using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbPool
{
    public List<Orb> orbs = new();
    public float rotateSpeed;
    public float distance = -1;
}

public class OrbManager : MonoBehaviour
{
    public static OrbManager instance;
    private const float OrbDistanceIncrement = 0.5f;

    Transform _orbPivot;
    [SerializeField] private string _label = "orb_config";
    [SerializeField] float _defaultDistance;
    [SerializeField] float _orbDamageModifier = 1f;
    [SerializeField] float _defaultRotateSpeed = 40f;
    
    int _generatedOrbSetCount = 0;
    float _rotateSpeed;
    AsyncOperationHandle<IList<OrbScriptable>> _handle;
    PoolManager _poolManager;

    Dictionary<EffectType, OrbScriptable> _orbSoDict = new();
    Dictionary<EffectType, OrbPool> _orbPoolDict = new();

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    void OnEnable()
    {
        EventBus.Subscribe(EventType.StageCombatStarted, FindOrbPivot);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe(EventType.StageCombatStarted, FindOrbPivot);
        ClearOrbData();
    }

    public async Awaitable LoadOrbConfigurationsAsync()
    {
        _rotateSpeed = _defaultRotateSpeed;

        CreateOrbObjDict();

        try
        {
            _handle = Addressables.LoadAssetsAsync<OrbScriptable>(
            _label,
            so => _orbSoDict[so.effectType] = so
            );

            await _handle.Task;
            destroyCancellationToken.ThrowIfCancellationRequested();

            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Load {_label} failed");
                ReleaseOrbHandle();
                throw new System.Exception($"Failed to load orb configurations with label: {_label}");
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogError($"[OrbManager] Load operation for {_label} was canceled.");
            throw new System.OperationCanceledException($"Load operation for {_label} was canceled.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OrbManager] Exception while loading {_label}: {ex.Message}");
            throw new System.Exception($"Exception while loading orb configurations with label: {_label}", ex);
        }
    }
    private void ReleaseOrbHandle()
    {
        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
            _handle = default;
        }
    }

    private void FindOrbPivot()
    {
        if (_orbPivot == null)
        {
            _orbPivot = FindAnyObjectByType<OrbPivot>().transform;
        }

        _poolManager = PoolManager.Instance;
    }

    private void CreateOrbObjDict()
    {
        _orbSoDict.Clear();

        foreach (EffectType type in Enum.GetValues(typeof(EffectType)))
        {
            _orbPoolDict[type] = new OrbPool();
        }
    }

    private void ClearOrbData()
    {
        ReleaseOrbHandle();

        foreach (var kv in _orbPoolDict)
        {
            var orbPool = kv.Value;
            foreach (var orb in orbPool.orbs)
            {
                if (orb != null)
                    _poolManager.ReturnObject(orb.gameObject);
            }

            orbPool.orbs.Clear();
            orbPool.distance = -1;
        }

        _orbSoDict.Clear();
    }

    public async Awaitable GenerateOrbAsync(EffectType type, int orbCount)
    {
        if (_orbPoolDict == null)
        {
            CreateOrbObjDict();
        }

        int count = orbCount - _orbPoolDict[type].orbs.Count;

        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            var orbSo = _orbSoDict[type];
            GameObject go = null;
            try
            {
                if (!PoolManager.Instance.TryGetObject(orbSo.orb, out go))
                {
                    go = await PoolManager.Instance.GetObjectAsync(orbSo.orb);
                }

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (go == null)
                {
                    Debug.LogError("[OrbManager] GenerateOrbAsync: 오브젝트를 풀에서 가져오지 못했습니다.");
                    continue;
                }

                go.transform.SetParent(_orbPivot);
                var orb = go.GetComponent<Orb>();
                _orbPoolDict[type].orbs.Add(orb);
            }
            catch (OperationCanceledException)
            {
                if (go != null)
                    PoolManager.Instance.ReturnObject(go);

                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OrbManager] 구슬 생성 중 치명적 에러 발생: {ex.Message}");

                if (go != null)
                    PoolManager.Instance.ReturnObject(go);

                continue;
            }
        }

        InitOrbType(type);
    }

    private void InitOrbType(EffectType type)
    {
        if (!_orbPoolDict.TryGetValue(type, out var orbPool) || orbPool.orbs is { Count : 0 })
            return;

        if (orbPool.distance <= 0)
        {
            _generatedOrbSetCount++;
            orbPool.distance = _defaultDistance + OrbDistanceIncrement * (_generatedOrbSetCount - 1);
            orbPool.rotateSpeed = _rotateSpeed;
            _rotateSpeed *= -1;
        }

        int count = orbPool.orbs.Count;

        for (int i = 0; i < count; i++)
        {
            var orb = orbPool.orbs[i];

            var config = new OrbConfig(_orbPivot, orbPool.rotateSpeed, type, _orbDamageModifier);
            orb.InitializeOrb(config);
            SetOrbPosition(orb.transform, orbPool.distance, count, i);
            orb.gameObject.SetActive(true);
        }
    }

    private void SetOrbPosition(Transform obj, float distance, int totalCount, int index)
    {
        Transform _pivot = _orbPivot;

        obj.position = _pivot.position + _pivot.forward * distance;

        float angle = (360f / totalCount) * index;
        obj.RotateAround(_pivot.position, Vector3.up, angle);
    }
}