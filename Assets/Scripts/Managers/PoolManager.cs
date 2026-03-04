using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        public Transform inactive;
        public Transform effectPool;
        public Transform projectilePool;
        public Transform enemyPool;
        public Transform mapPool;
        public Transform extra;
    
        class Pool
        {
            public AsyncOperationHandle<GameObject> prefabHandle;
            public GameObject prefab;
            public Transform root;
        
            public readonly HashSet<GameObject> all = new();
            public readonly Queue<GameObject> inactive = new();
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

        public async Awaitable PrewarmAsync(AssetReferenceGameObject aref, int count = 5)
        {
            string key = aref.AssetGUID;
        
            await EnsurePoolLoadedAsync(key, aref);
            destroyCancellationToken.ThrowIfCancellationRequested();
        
            var pool = _pools[key];
            pool.root = inactive;

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(pool.prefab, pool.root, false);
                go.SetActive(false);
            
                EnsureTag(go, key);
                _instanceToKey[go] = key;
            
                pool.all.Add(go);
                pool.inactive.Enqueue(go);
            }
        }
    
        public bool TryGetObject(
            AssetReferenceGameObject aref,
            out GameObject instance,
            Transform parentOverride = null)
        {
            instance = null;
        
            if (!_pools.TryGetValue(aref.AssetGUID, out var pool))
                return false;

            while (pool.inactive.Count > 0 && !instance)
            { 
                var go = pool.inactive.Dequeue();
            
                if (!go) continue;
            
                instance = go;
                instance.transform.SetParent(parentOverride, false);

                return true;
            }
            return false;
        }
    
        public async Awaitable<GameObject> GetObjectAsync(
            AssetReferenceGameObject aref,
            Transform parentOverride = null,
            int extraPrewarmCount = 4
        )
        {
            string key = aref.AssetGUID;
            try
            {
                await EnsurePoolLoadedAsync(key, aref);
                destroyCancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"GetObject cancelled during pool loading: {key}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load pool for key: {key}, Exception: {ex}");
                return null;
            }
            
            var pool = _pools[key];
        
            while (pool.inactive.Count > 0)
            {
                var go = pool.inactive.Dequeue();
                if (!go) continue;
            
                go.transform.SetParent(parentOverride, false);
                go.SetActive(false);
            
                return go;
            }
        
            var instance = Instantiate(pool.prefab, pool.root, false);
            if (parentOverride != null)
            {
                instance.transform.SetParent(parentOverride, false);
            }
        
            instance.SetActive(false);
        
            EnsureTag(instance, key);
            _instanceToKey[instance] = key;
        
            pool.all.Add(instance);
        
            if (extraPrewarmCount > 0)
                CreateAndEnqueue(pool, key, extraPrewarmCount);

            return instance;
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
            instance.transform.SetParent(pool.root, false);
            pool.inactive.Enqueue(instance);
        }
    
        public void ClearAllPools()
        {
            foreach (var kv in _pools)
            {
                var pool = kv.Value;
            
                foreach (var go in pool.all)
                    if (go != null) Destroy(go);

                pool.all.Clear();
                pool.inactive.Clear();
            
                if (pool.prefabHandle.IsValid())
                    Addressables.Release(pool.prefabHandle);
            }
            _pools.Clear();
            _instanceToKey.Clear();
        }
    
        private async Awaitable EnsurePoolLoadedAsync(string key, AssetReferenceGameObject aref)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = new Pool();
                _pools.Add(key, pool);
            }
        
            if (!pool.prefabHandle.IsValid())
            {
                try
                {
                    pool.prefabHandle = aref.LoadAssetAsync<GameObject>();
                    await pool.prefabHandle.Task;

                    if (pool.prefabHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        throw new Exception($"Addressables LoadAssetAsync failed: {key}");
                    }

                    pool.prefab = pool.prefabHandle.Result;
                    pool.root = inactive;

                    destroyCancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    if (pool.prefabHandle.IsValid())
                    {
                        Addressables.Release(pool.prefabHandle);
                    }

                    throw;
                }
            }
        }
    
        private void CreateAndEnqueue(Pool pool, string key, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(pool.prefab, pool.root, false);
                go.SetActive(false);
            
                EnsureTag(go, key);
                _instanceToKey[go] = key;
                pool.all.Add(go);
                pool.inactive.Enqueue(go);
            }
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