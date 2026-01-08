using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        public Transform Inactive;
        public Transform EffectPool;
        public Transform ProjectilePool;
        public Transform EnemyPool;
        public Transform MapPool;
        public Transform Extra;
    
        class Pool
        {
            public AsyncOperationHandle<GameObject> PrefabHandle;
            public GameObject Prefab;
            public Transform Root;
        
            public readonly HashSet<GameObject> All = new();
            public readonly Queue<GameObject> Inactive = new();
        }
    
        private readonly Dictionary<string, Pool> _pools = new();
        private readonly Dictionary<GameObject, string> _instanceToKey = new();

        [Serializable]
        class PoolTag : MonoBehaviour { public string key; }

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator Prewarm(AssetReferenceGameObject aref, int count = 5)
        {
            string key = aref.AssetGUID;
        
            yield return EnsurePoolLoaded(key, aref);
        
            var pool = _pools[key];
            pool.Root = Inactive;

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(pool.Prefab, pool.Root, false);
                go.SetActive(false);
            
                EnsureTag(go, key);
                _instanceToKey[go] = key;
            
                pool.All.Add(go);
                pool.Inactive.Enqueue(go);
            }
        }
    
        public bool TryGetObject(AssetReferenceGameObject aref, out GameObject instance, Transform parentOverride = null)
        {
            instance = null;
        
            if (!_pools.TryGetValue(aref.AssetGUID, out var pool))
                return false;

            while (pool.Inactive.Count > 0 && !instance)
            { 
                var go = pool.Inactive.Dequeue();
            
                if (!go) continue;
            
                instance = go;
                instance.transform.SetParent(parentOverride);

                return true;
            }
            return false;
        }
    
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
        
            while (pool.Inactive.Count > 0)
            {
                var go = pool.Inactive.Dequeue();
                if (!go) continue;
            
                go.transform.SetParent(parentOverride, false);
                go.SetActive(false);
            
                onReady?.Invoke(go);
                yield break;
            }
        
            var instance = Instantiate(pool.Prefab, pool.Root, false);
            if (parentOverride != null)
            {
                instance.transform.SetParent(parentOverride, false);
            }
        
            instance.SetActive(false);
        
            EnsureTag(instance, key);
            _instanceToKey[instance] = key;
        
            pool.All.Add(instance);

            onReady?.Invoke(instance);
        
            if (extraPrewarmCount > 0)
                StartCoroutine(CreateAndEnqueue(pool, key, extraPrewarmCount));
        }
    
        public void ReturnObject(GameObject instance)
        {
            if (!instance) return;

            if (!_instanceToKey.TryGetValue(instance, out var key) || !_pools.TryGetValue(key, out var pool))
            {
                instance.SetActive(false);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(pool.Root, false);
            pool.Inactive.Enqueue(instance);
        }
    
        public void ClearAllPools()
        {
            foreach (var kv in _pools)
            {
                var pool = kv.Value;
            
                foreach (var go in pool.All)
                    if (go != null) Destroy(go);

                pool.All.Clear();
                pool.Inactive.Clear();  
            
                if (pool.PrefabHandle.IsValid())
                    Addressables.Release(pool.PrefabHandle);
            }
            _pools.Clear();
            _instanceToKey.Clear();
        }
    
        private IEnumerator EnsurePoolLoaded(string key, AssetReferenceGameObject aref)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = new Pool();
                _pools.Add(key, pool);
            }
        
            if (!pool.PrefabHandle.IsValid())
            {
                pool.PrefabHandle = aref.LoadAssetAsync<GameObject>();
                yield return pool.PrefabHandle;

                if (pool.PrefabHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception($"Addressables LoadAssetAsync failed: {key}");
                }

                pool.Prefab = pool.PrefabHandle.Result;
                pool.Root   = Inactive;
            }

            yield return pool;
        }
    
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
            }
            yield break;
        }

        private static void EnsureTag(GameObject go, string key)
        {
            var tag = go.GetComponent<PoolTag>();
            if (!tag) 
                tag = go.AddComponent<PoolTag>();
        
            tag.key = key;
        }
    }
}
