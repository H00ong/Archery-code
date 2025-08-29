using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PoolManager : MonoBehaviour
{
    [SerializeField] Transform Inactive;
    // 내부 풀 구조 (프리팹당 1 핸들만 유지)
    class Pool
    {
        public AsyncOperationHandle<GameObject> PrefabHandle; // LoadAssetAsync 핸들(프리팹 에셋)
        public GameObject Prefab;                              // PrefabHandle.Result
        public Transform Root;                                 // 비활성 루트 (항상 SetActive(false))
        public readonly HashSet<GameObject> All = new();       // 관리 중인 전체 인스턴스
        public readonly Queue<GameObject> Inactive = new();    // 재사용 대기열
    }

    // 키: Addressables GUID (AssetReference.AssetGUID)
    private readonly Dictionary<string, Pool> _pools = new();
    // 인스턴스 → 소속 풀 GUID 역매핑
    private readonly Dictionary<GameObject, string> _instanceToKey = new();

    [Serializable]
    class PoolTag : MonoBehaviour { public string key; }

    // ---------- Public API ----------

    /// <summary>
    /// 프리팹 로드 + count개 예열(비활성 루트 아래 적재)
    /// parent는 "지급 시점"에만 쓰도록 하고, 생성/보관은 항상 비활성 루트 아래에서 수행합니다.
    /// </summary>

    public IEnumerator Prewarm(AssetReferenceGameObject aref, int count = 5)
    {
        string key = aref.AssetGUID;
        yield return EnsurePoolLoaded(key, aref); // StartCoroutine 생략해도 됨
        var pool = _pools[key];                   // 여기서 안전하게 참조
        pool.Root = Inactive;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(pool.Prefab, pool.Root, false);
            go.SetActive(false);
            EnsureTag(go, key);
            _instanceToKey[go] = key;
            pool.All.Add(go);
            pool.Inactive.Enqueue(go);
            // 필요 시 프레임 분산:
            // yield return null;
        }
    }

    /// <summary>
    /// 즉시 할당 시도(대기 X). 성공 시 instance 반환.
    /// 지급 시점에는 부모 이동만 수행하세요(활성화는 호출자 정책에 따름).
    /// 없을 시에는 그냥 객체 가지고 오는 것을 포기한다. 즉 prewarm으로 만들어놓을 때 같이 쓰는 용도
    /// </summary>
    public bool TryGetObject(AssetReferenceGameObject aref, out GameObject instance, Transform parentOverride = null)
    {
        instance = null;
        if (!_pools.TryGetValue(aref.AssetGUID, out var pool))
            return false;

        while (pool.Inactive.Count > 0 && instance == null)
        {
            if (parentOverride != null) pool.Root = parentOverride;
            var go = pool.Inactive.Dequeue();
            if (go == null) continue; // 이미 파괴된 경우 방어
            instance = go;

            if (parentOverride != null)
                instance.transform.SetParent(pool.Root);

            return true;
        }
        return false;
    }

    /// <summary>
    /// 없으면 생성해서 넘겨줌(대기 O). onReady는 "비활성 상태"로 콜백합니다.
    /// extraPrewarmCount만큼 추가 예열을 백그라운드로 진행합니다.
    /// </summary>
    public IEnumerator GetObject(
        AssetReferenceGameObject aref,
        Action<GameObject> onReady,
        Transform parentOverride = null,
        int extraPrewarmCount = 4
    )
    {
        string key = aref.AssetGUID;
        yield return EnsurePoolLoaded(key, aref);
        var pool = _pools[key];                   

        // 1) 비활성 큐 우선 지급
        while (pool.Inactive.Count > 0)
        {
            var go = pool.Inactive.Dequeue();
            if (go == null) continue;

            if (parentOverride != null)
                go.transform.SetParent(parentOverride, false);

            // 지급은 항상 비활성로. 켜는 타이밍은 호출자 책임.
            go.SetActive(false);
            onReady?.Invoke(go);
            yield break;
        }

        // 2) 없으면 1개 생성해서 즉시 지급(비활성 상태)
        if(parentOverride != null) pool.Root = parentOverride;
        var instance = Instantiate(pool.Prefab, pool.Root, false);
        instance.SetActive(false);
        EnsureTag(instance, key);
        _instanceToKey[instance] = key;
        pool.All.Add(instance);

        onReady?.Invoke(instance);

        // 3) 백그라운드 예열(비활성 루트 아래 적재)
        if (extraPrewarmCount > 0)
            StartCoroutine(CreateAndEnqueue(pool, key, extraPrewarmCount));
    }

    /// <summary>
    /// 반납: 비활성 루트로 복귀 + 큐 적재.
    /// </summary>
    public void ReturnObject(GameObject instance)
    {
        if (instance == null) return;

        if (!_instanceToKey.TryGetValue(instance, out var key) || !_pools.TryGetValue(key, out var pool))
        {
            // 풀 소속이 아니면 방어적으로 끄기만.
            instance.SetActive(false);
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(pool.Root, false);
        pool.Inactive.Enqueue(instance);
    }

    /// <summary>
    /// 모든 풀 정리: 인스턴스 Destroy, 프리팹 핸들 Release, 루트 파괴.
    /// (LoadAssetAsync 경로만 취급)
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kv in _pools)
        {
            var pool = kv.Value;

            // 인스턴스 파괴
            foreach (var go in pool.All)
                if (go != null) Destroy(go);
            pool.All.Clear();
            pool.Inactive.Clear();

            // 프리팹 핸들 Release (단 1회)
            if (pool.PrefabHandle.IsValid())
                Addressables.Release(pool.PrefabHandle);
        }
        _pools.Clear();
        _instanceToKey.Clear();
    }

    // ---------- Internals ----------

    // 프리팹 핸들과 비활성 루트를 보장
    private IEnumerator EnsurePoolLoaded(string key, AssetReferenceGameObject aref)
    {
        if (!_pools.TryGetValue(key, out var pool))
        {
            pool = new Pool();
            _pools.Add(key, pool);
        }

        // 프리팹 핸들 준비(1회 로드)
        if (!pool.PrefabHandle.IsValid())
        {
            pool.PrefabHandle = aref.LoadAssetAsync<GameObject>(); // or Addressables.LoadAssetAsync<GameObject>(key)
            yield return pool.PrefabHandle;

            if (pool.PrefabHandle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"Addressables LoadAssetAsync failed: {key}");

            pool.Prefab = pool.PrefabHandle.Result;
            pool.Root   = Inactive;
        }

        yield return pool;
    }

    // 비동기 예열: count개 생성 → 비활성 루트에 적재
    private IEnumerator CreateAndEnqueue(Pool pool, string key, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(pool.Prefab, pool.Root, false);
            go.SetActive(false);
            EnsureTag(go, key);
            _instanceToKey[go] = key;
            pool.All.Add(go);
            pool.Inactive.Enqueue(go);

            // 필요 시 프레임 분산
            // yield return null;
        }
        yield break;
    }

    private static void EnsureTag(GameObject go, string key)
    {
        var tag = go.GetComponent<PoolTag>();
        if (tag == null) tag = go.AddComponent<PoolTag>();
        tag.key = key;
    }
}
