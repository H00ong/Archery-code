using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;

        [Header("Settings")]
        [SerializeField] private Vector3 defaultMapPosition;
        [SerializeField] private string label = "map_config";

        [HideInInspector] public MapData currentMapData = null;
        [HideInInspector] public MapType currentMapType;
        [HideInInspector] public GameMap currentMap = null;
        
        private PoolManager _poolManager;
        private DataManager _dataManager;
        
        // Addressable & Caching
        private AsyncOperationHandle<IList<MapScriptable>> _handle;
        private Dictionary<MapType, MapScriptable> _mapDict = new Dictionary<MapType, MapScriptable>();
        
        private List<AssetReferenceGameObject> _currentMapList;
        private List<AssetReferenceGameObject> _currentEnemyList;
        private List<AssetReferenceGameObject> _currentBossList;
        private AssetReferenceGameObject _currentBossMapRef;

        private readonly List<GameObject> _preloadedMaps = new List<GameObject>();
        private GameObject _preloadedBossMap;

        // 이벤트 핸들러
        private Action _onStartedLoading;

        public int CurrentMapIndex { get; private set; } = 0;

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

        private void Start()
        {
            _poolManager = PoolManager.Instance;
            _dataManager = DataManager.Instance;

            currentMap = null;
            currentMapData = null;

            StartCoroutine(PrepareMapRoutine()); 
        }

        private void OnDestroy()
        {
            if (_handle.IsValid()) Addressables.Release(_handle);
            
            _currentBossList.Clear();
            _currentEnemyList.Clear();
            _currentMapList.Clear();
            _currentBossMapRef = null;
            
            _mapDict.Clear();
            _preloadedMaps.Clear();

            currentMap = null;
            currentMapData = null;
        }

        #region Main Loading Sequence

        // 모든 로딩 과정을 순서대로 처리하는 메인 코루틴
        // 초기화 및 맵 생성 (준비만 함)
        private IEnumerator PrepareMapRoutine()
        {
            yield return LoadMapConfigRoutine();
            
            RefreshMapData();

            yield return PreloadMapsRoutine();
            
            StageManager.Instance.Init(currentMapData);
        }
        #endregion

        #region 1. Data & Addressables

        private IEnumerator LoadMapConfigRoutine()
        {
            if (_mapDict is { Count: > 0 }) yield break;

            _mapDict = new Dictionary<MapType, MapScriptable>();
            
            _handle = Addressables.LoadAssetsAsync<MapScriptable>(
                label,
                so => { if (so) _mapDict[so.mapType] = so; }
            );

            yield return _handle;

            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[MapManager] Addressable Load Failed: {label}");
            }
        }

        private void RefreshMapData()
        {
            if(currentMapData != null) return;
            
            currentMapData = _dataManager.GetMapData(CurrentMapIndex);
            
            if (currentMapData == null)
            {
                Debug.LogError("MapData is null!");
                return;
            }

            currentMapType = (MapType)currentMapData.mapType;

            if (!_mapDict.TryGetValue(currentMapType, out var scriptable))
            {
                Debug.LogError($"MapScriptable not found for type: {currentMapType}");
            }

            if (scriptable == null) return;
            
            _currentMapList = scriptable.mapList;
            _currentEnemyList = scriptable.enemyList;
            _currentBossList = scriptable.bossList;
            _currentBossMapRef = scriptable.bossMap;
        }

        #endregion

        #region 2. Map Pooling (Creation)

        public IEnumerator PreloadMapsRoutine()
        {
            if (_preloadedMaps is { Count: > 0 })  yield break;

            yield return new WaitUntil(() => _currentMapList.Count > 0);

            foreach (var mapRef in _currentMapList)
            {
                if (!_poolManager.TryGetObject(mapRef, out var go, _poolManager.MapPool))
                {
                    yield return _poolManager.GetObject(mapRef, inst => go = inst, _poolManager.MapPool);
                }

                if (!go) continue;
                
                _preloadedMaps.Add(go);
            }

            // 보스 맵 생성 요청
            if (_currentBossMapRef == null)
            {
                Debug.LogError("_currentBossMapRef is null.");
                yield break;
            }

            if (!_poolManager.TryGetObject(_currentBossMapRef, out _preloadedBossMap, _poolManager.MapPool))
            {
                yield return _poolManager.GetObject(_currentBossMapRef, 
                    inst => _preloadedBossMap = inst,
                    _poolManager.MapPool);
            }
        }

        #endregion

        #region 3. Map Activation

        // 맵 활성화 (StageManager가 시킬 때 수행)
        public void ActivateRandomMap(bool isBoss)
        {
            if (currentMap != null)
            {
                currentMap.gameObject.SetActive(false);
            }
            
            SelectMap(isBoss);
            
            currentMap.transform.position = defaultMapPosition;
            currentMap.Init();
            currentMap.gameObject.SetActive(true);
        }

        private void SelectMap(bool isBoss)
        {
            if (isBoss)
            {
                if (_preloadedBossMap == null)
                {
                    Debug.LogError("_preloadedBossMap is null.");
                    return;
                }

                currentMap = _preloadedBossMap.GetComponent<GameMap>();
                return;
            }

            if (_preloadedMaps == null || _preloadedMaps.Count == 0)
            {
                Debug.LogError("_preloadedMaps is null or empty.");
                return;
            }

            int idx = UnityEngine.Random.Range(0, _preloadedMaps.Count);
            currentMap = _preloadedMaps[idx].GetComponent<GameMap>();
            currentMap.Init();
        }

        #endregion

        #region 4. Player & Enemy

        public Transform GetPlayerSpawnPoint()
        {
            if (!currentMap)
            {
                Debug.LogError("Cannot position player: currentMap is null.");
                return null;
            }

            return currentMap.PlayerSpawnPoint;
        }

        public List<Transform> GetEnemySpawnPoint(int count)
        {
            var list = currentMap.enemySpawnPoints;
            
            if (list.Count < count)
            {
                Debug.LogError($"[MapManager] Not enough spawn points! Requested: {count}, Available: {list.Count}");
                return null;
            }
            
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
            
            return list.GetRange(0, count);
        }

        public Transform GetBossSpawnPoint()
        {
            return currentMap.bossSpawnPoint;
        }

        public AssetReferenceGameObject GetBossAssetRef(int index)
        {
            return _currentBossList[index];
        }

        public AssetReferenceGameObject GetEnemeyAssetRef()
        {
            var count = _currentEnemyList.Count;
            var idx = UnityEngine.Random.Range(0, count);
            
            return _currentEnemyList[idx];
        }

        #endregion

        public void ClearMap()
        {
            if (currentMap == null) return;
            
            currentMap.gameObject.SetActive(false);
            _poolManager.ReturnObject(currentMap.gameObject);
            currentMap = null;
        }
    }
}