using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Players;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Vector3 defaultMapPosition;
        [SerializeField] private string label = "map_config";

        private string _currentMapID;
        private GameMap currentMap = null;

        private PoolManager _poolManager;
        private DataManager _dataManager;
        
        // Addressable & Caching
        private AsyncOperationHandle<IList<MapScriptable>> _handle;
        private Dictionary<string, MapScriptable> _mapDict = new Dictionary<string, MapScriptable>();
        
        private List<AssetReferenceGameObject> _currentMapList;
        private AssetReferenceGameObject _currentBossMapRef;
        
        // EnemyIdentity 기반 리스트
        private List<EnemyIdentity> _currentEnemyIdentityList;
        private List<EnemyIdentity> _currentBossIdentityList;

        private readonly List<GameObject> _preloadedMaps = new List<GameObject>();
        private GameObject _preloadedBossMap;

        // TODO : json 저장
        public int CurrentMapIndex { get; private set; } = 0;
        public int MaxMapIndex { get; private set; } = 0;
        public MapData CurrentMapData { get; private set; }

        // TODO : MaxMapIndex를 갱신한다
        public void UpdateMapClearData()
        {
            if (CurrentMapIndex > MaxMapIndex)
                MaxMapIndex = CurrentMapIndex;
        }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        void Start()
        {
            _poolManager = PoolManager.Instance;
            _dataManager = DataManager.Instance;
        }

        void OnEnable()
        {
            EventBus.Subscribe(EventType.TransitionToLobby, OnStageMapClear);
            EventBus.Subscribe(EventType.Retry, OnStageMapClear);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(EventType.TransitionToLobby, OnStageMapClear);
            EventBus.Unsubscribe(EventType.Retry, OnStageMapClear);

            if (_handle.IsValid())
                Addressables.Release(_handle);

            OnStageMapClear();
        }

        private void OnStageMapClear()
        {
            _currentMapList?.Clear();
            _preloadedMaps?.Clear();
            _currentBossMapRef = null;
            _currentEnemyIdentityList?.Clear();
            _currentBossIdentityList?.Clear();

            if(currentMap != null)
                PoolManager.Instance.ReturnObject(currentMap.gameObject);

            currentMap = null;
            CurrentMapData = null;
        }
        
        #region 1. Data & Addressables

        public async Awaitable LoadMapConfigAsync()
        {
            currentMap = null;
            CurrentMapData = null;

            if (_mapDict is { Count: > 0 }) return;

            _mapDict = new Dictionary<string, MapScriptable>();
            try
            {
                _handle = Addressables.LoadAssetsAsync<MapScriptable>(
                    label,
                    so => { if (so) _mapDict[so.mapId] = so; }
                );

                await _handle.Task;

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (_handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[MapManager] 맵 설정 로드 실패: {label}");
                    throw new InvalidOperationException($"[MapManager] Addressable load failed for label: {label}");
                }
                else
                {
                    Debug.Log($"[MapManager] 맵 설정 로드 성공: {label}, 총 맵 종류: {_mapDict.Count}");
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.LogError($"[MapManager] OperationCanceledException during LoadMapConfigAsync: {label}");
                if (_handle.IsValid())
                    Addressables.Release(_handle);
                    
                throw;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapManager] Exception during LoadMapConfigAsync: {label}, Exception: {ex}");
                if (_handle.IsValid())
                    Addressables.Release(_handle);
                throw;
            }
        }

        public void SetCurrentMapIndex(int index)
        {
            CurrentMapIndex = index;
            Debug.Log($"[MapManager] CurrentMapIndex set to: {CurrentMapIndex}");
        }

        public void RefreshMapData()
        {
            CurrentMapData = _dataManager.GetMapData(CurrentMapIndex);

            if (CurrentMapData == null)
            {
                throw new System.Exception($"[MapManager] MapData is null for index: {CurrentMapIndex}");
            }

            _currentMapID = CurrentMapData.mapId;

            if (!_mapDict.TryGetValue(_currentMapID, out var mapSo) || mapSo == null)
            {
                throw new InvalidOperationException($"[MapManager] MapScriptable not found for mapId: {_currentMapID}");
            }

            _currentMapList = mapSo.mapList.ToList();
            _currentBossMapRef = mapSo.bossMap;

            _currentEnemyIdentityList = mapSo.enemyIdentityList.ToList();
            _currentBossIdentityList = mapSo.bossIdentityList.ToList();
        }

        #endregion

        #region 2. Map Pooling (Creation)

        public async Awaitable PreloadMapsAsync()
        {
            if (_preloadedMaps is { Count: > 0 })
                return;

            try
            {
                foreach (var mapRef in _currentMapList)
                {
                    if (!_poolManager.TryGetObject(mapRef, out var go, _poolManager.mapPool))
                    {
                        go = await _poolManager.GetObjectAsync(mapRef, _poolManager.mapPool);
                    }

                    destroyCancellationToken.ThrowIfCancellationRequested();

                    if (!go)
                    {
                        throw new System.Exception($"[MapManager] 맵 프리팹 풀링 실패: {mapRef}");
                    }

                    _preloadedMaps.Add(go);
                }

                if (!_poolManager.TryGetObject(_currentBossMapRef, out _preloadedBossMap, _poolManager.mapPool))
                {
                    _preloadedBossMap = await _poolManager.GetObjectAsync(_currentBossMapRef, _poolManager.mapPool);
                }

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (!_preloadedBossMap)
                {
                    throw new System.Exception("[MapManager] 보스 맵 프리팹 풀링 실패.");
                }
            }
            catch (System.OperationCanceledException)
            {
                CleanUpPreloadedMaps();
                throw;
            }
            catch (System.Exception ex)
            {
                CleanUpPreloadedMaps();
                throw new System.Exception($"[MapManager] 맵 프리로드 중 치명적 에러: {ex.Message}", ex);
            }
        }

        private void CleanUpPreloadedMaps()
        {
            foreach (var map in _preloadedMaps)
            {
                if (map != null)
                    _poolManager.ReturnObject(map);
            }

            if (_preloadedBossMap != null)
            {
                _poolManager.ReturnObject(_preloadedBossMap);
                _preloadedBossMap = null;
            }
        }

        #endregion

        #region 3. Map Activation
        public void ActivateRandomMap(bool isBoss)
        {
            if (currentMap != null)
            {
                currentMap.gameObject.SetActive(false);
            }
            
            SelectMap(isBoss);
            
            currentMap.gameObject.SetActive(true);
            currentMap.Init();
            currentMap.transform.position = defaultMapPosition;
        }

        private void SelectMap(bool isBoss)
        {
            if (isBoss)
            {
                currentMap = _preloadedBossMap.GetComponent<GameMap>();
                return;
            }

            int idx = UnityEngine.Random.Range(0, _preloadedMaps.Count);
            currentMap = _preloadedMaps[idx].GetComponent<GameMap>();
        }

        #endregion

        #region 4. Get Methods

        public Transform GetPlayerSpawnPoint()
        {
            return currentMap.PlayerSpawnPoint;
        }

        public List<Transform> GetEnemySpawnPoints(int count)
        {
            var list = currentMap.EnemySpawnPoints;
            int availableCount = list.Count > count ? count : list.Count;

            for (int i = 0; i < availableCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
            
            return list.GetRange(0, availableCount);
        }

        public Transform GetBossSpawnPoint()
        {
            return currentMap.BossSpawnPoint;
        }

        public EnemyIdentity GetEnemyIdentity()
        {
            var count = _currentEnemyIdentityList.Count;
            var idx = UnityEngine.Random.Range(0, count);
            
            return _currentEnemyIdentityList[idx];
        }
        
        public EnemyIdentity GetBossIdentity(int index)
        {
            return _currentBossIdentityList[index];
        }

        public Vector3 GetRandomPointInMap()
        {
            return currentMap.GetRandomNavMeshPoint();
        }

        public Vector3 GetMapPosition()
        {
            return currentMap.transform.position;
        }

        public List<Vector3> GetPatrolPositions()
        {
            return currentMap != null ? currentMap.GetPatrolPositions() : null;
        }

        public List<PatrolPoint> GetAllPatrolPoints()
        {
            return currentMap != null ? currentMap.GetAllPatrolPoints() : null;
        }

        public List<EnemySpawnData> GetPredefinedEnemies()
        {
            return currentMap != null ? currentMap.PredefinedEnemies : null;
        }

        #endregion
    }
}