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

        
        [HideInInspector] public MapType currentMapType;
        [HideInInspector] public GameMap currentMap = null;

        private MapData _currentMapData = null;

        private PoolManager _poolManager;
        private DataManager _dataManager;
        
        // Addressable & Caching
        private AsyncOperationHandle<IList<MapScriptable>> _handle;
        private Dictionary<MapType, MapScriptable> _mapDict = new Dictionary<MapType, MapScriptable>();
        
        private List<AssetReferenceGameObject> _currentMapList;
        private List<AssetReferenceGameObject> _currentEnemyList;
        private List<AssetReferenceGameObject> _currentBossList;
        private AssetReferenceGameObject _currentBossMapRef;
        
        // EnemyIdentity 기반 리스트
        private List<EnemyIdentity> _currentEnemyIdentityList;
        private List<EnemyIdentity> _currentBossIdentityList;

        private readonly List<GameObject> _preloadedMaps = new List<GameObject>();
        private GameObject _preloadedBossMap;

        // 이벤트 핸들러
        private Action _onStartedLoading;

        public int CurrentMapIndex { get; private set; } = 0;
        public MapData CurrentMapData => _currentMapData;

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
            _currentMapData = null;

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
            _currentMapData = null;
        }

        #region Main Loading Sequence

        // 모든 로딩 과정을 순서대로 처리하는 메인 코루틴
        // 초기화 및 맵 생성 (준비만 함)
        private IEnumerator PrepareMapRoutine()
        {
            yield return LoadMapConfigRoutine();
            
            RefreshMapData();

            yield return PreloadMapsRoutine();
            
            StageManager.Instance.Init(_currentMapData);
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
            if(_currentMapData != null) return;
            
            _currentMapData = _dataManager.GetMapData(CurrentMapIndex);
            
            if (_currentMapData == null)
            {
                Debug.LogError("MapData is null!");
                return;
            }

            currentMapType = (MapType)_currentMapData.mapType;

            if (!_mapDict.TryGetValue(currentMapType, out var mapSo))
            {
                Debug.LogError($"MapScriptable not found for type: {currentMapType}");
            }

            if (mapSo == null) return;
            
            _currentMapList = mapSo.mapList;
            _currentEnemyList = mapSo.enemyList;
            _currentBossList = mapSo.bossList;
            _currentBossMapRef = mapSo.bossMap;
            
            // EnemyIdentity 기반 리스트 초기화
            _currentEnemyIdentityList = mapSo.enemyIdentityList;
            _currentBossIdentityList = mapSo.bossIdentityList;
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
        
        /// <summary>
        /// EnemyIdentity 기반으로 랜덤 적 정보 반환
        /// </summary>
        public EnemyIdentity GetEnemyIdentity()
        {
            if (_currentEnemyIdentityList == null || _currentEnemyIdentityList.Count == 0)
                return null;
                
            var count = _currentEnemyIdentityList.Count;
            var idx = UnityEngine.Random.Range(0, count);
            
            return _currentEnemyIdentityList[idx];
        }
        
        /// <summary>
        /// EnemyIdentity 기반으로 보스 정보 반환
        /// </summary>
        public EnemyIdentity GetBossIdentity(int index)
        {
            if (_currentBossIdentityList == null || _currentBossIdentityList.Count == 0)
                return null;
                
            if (index < 0 || index >= _currentBossIdentityList.Count)
                return null;
                
            return _currentBossIdentityList[index];
        }
        
        /// <summary>
        /// EnemyIdentity 리스트가 설정되어 있는지 확인
        /// </summary>
        public bool HasEnemyIdentityList => _currentEnemyIdentityList != null && _currentEnemyIdentityList.Count > 0;

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