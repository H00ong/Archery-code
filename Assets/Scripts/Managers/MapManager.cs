using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapManager : MonoBehaviour
{
    [Header("Default Postion")]
    [SerializeField] Vector3 _defaultMapPosition;

    public static MapData CurrentMapData = null;
    public static int CurrentMapIndex = 0;
    public static MapType CurrentMapType;
    public static GameObject CurrentMap;
    private List<GameObject> _createdMaps;

    [SerializeField] private string _label = "map_config"; // Addressables 공통 라벨
    private Dictionary<MapType, MapScriptable> _mapDict = new Dictionary<MapType, MapScriptable>();
    private AsyncOperationHandle<IList<MapScriptable>> _handle; // 핸들 유지(언로드 방지)

    private List<AssetReferenceGameObject> _mapList;
    private List<AssetReferenceGameObject> _mapEnemyList;
    private List<AssetReferenceGameObject> _bossEnemyList;
    private AssetReferenceGameObject _bossMapRef;
    private GameObject _bossMap; // 보스맵 딱 하나만 존재하고 따로 보관

    private bool _mapsReady = false;
    private bool _isCreatingMaps = false;

    public static event Action OnMapUpdated;
    private Action _onStageCleared;

    private PoolManager _poolManager;
    private DataManager _dataManager;

    private void Start()
    {
        Init();
    }

    public void Init() 
    {
        _poolManager = PoolManager.Instance;
        _dataManager = DataManager.Instance;

        StartCoroutine(CreateAllMaps());
    }

    private void OnEnable()
    {
        _onStageCleared = () =>
        {
            GetNewMap(StageManager.IsBossStage);
            SpawnEnemy(isBoss: StageManager.IsBossStage);
        };

        StageManager.OnStageCleared += _onStageCleared;
        StageManager.OnStageCleared += PositionPlayer;
    }

    private void OnDisable()
    {
        StageManager.OnStageCleared -= _onStageCleared;
        StageManager.OnStageCleared -= PositionPlayer;
    }

    #region Map
    private IEnumerator EnsureMapDictReady()
    {
        if (_mapDict != null && _mapDict.Count > 0)
            yield break;

        _mapDict.Clear();
        _mapDict ??= new Dictionary<MapType, MapScriptable>();

        _handle = Addressables.LoadAssetsAsync<MapScriptable>(
            _label,
            so => _mapDict[so.mapType] = so // 동일 key 존재 시 마지막 항목으로 갱신
        );
        yield return _handle;

        if (_handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError($"[MapManager] Addressables Load failed. label={_label}");
    }

    private void GetMapDataFromDataManager()
    {
        if (CurrentMapData != null) return;

        CurrentMapData = _dataManager.GetMapData(CurrentMapIndex);
        if (CurrentMapData == null) return;

        CurrentMapType = (MapType)CurrentMapData.mapType;
        if (!_mapDict.TryGetValue(CurrentMapType, out var data))
        {
            Debug.LogError($"[MapManager] MapScriptable not found for {CurrentMapType}");
            return;
        }

        // 참조 복사 대신 새 리스트로 교체(외부에서 Clear()해도 원본 SO 영향 없음)
        _mapList = new List<AssetReferenceGameObject>(data.mapList);
        _mapEnemyList = new List<AssetReferenceGameObject>(data.enemyList);
        _bossEnemyList = new List<AssetReferenceGameObject>(data.bossList);
        _bossMapRef = data.bossMap;
    }

    public void GetNewMap(bool isBossMap = false)
    {
        StartCoroutine(GetMapCoroutine(go =>
        {
            if (go == null) return;
            else
            {
                if (CurrentMap != null)
                    CurrentMap.SetActive(false); // 기존 맵 비활성화

                CurrentMap = go;
                CurrentMap.GetComponent<Map>().Init();
                CurrentMap.transform.position = _defaultMapPosition;
                CurrentMap.SetActive(true);

                OnMapUpdated?.Invoke();
            }
        }, isBossMap));
    }

    public IEnumerator GetMapCoroutine(Action<GameObject> onReady, bool isBossMap = false)
    {
        yield return EnsureMapsReady();

        if (!isBossMap)
        {
            if (_createdMaps == null || _createdMaps.Count == 0)
            {
                Debug.LogError("No maps created.");
                onReady?.Invoke(null);
                yield break;
            }

            int idx = UnityEngine.Random.Range(0, _createdMaps.Count);
            onReady?.Invoke(_createdMaps[idx]);
        }
        else
        {
            if (_bossMap == null)
            {
                Debug.LogError("Boss maps not created");
                onReady?.Invoke(null);
                yield break;
            }

            onReady?.Invoke(_bossMap);
        }
    }

    private IEnumerator EnsureMapsReady()
    {
        if (_mapsReady) yield break;
        if (!_isCreatingMaps) yield return StartCoroutine(CreateAllMaps());
        else yield return new WaitUntil(() => _mapsReady);
    }

    public IEnumerator CreateAllMaps()
    {
        if (_mapsReady || _isCreatingMaps) yield break;
        _isCreatingMaps = true;

        yield return EnsureMapDictReady();
        GetMapDataFromDataManager();

        if (_createdMaps != null) _createdMaps.Clear();
        else _createdMaps = new List<GameObject>();

        foreach (var map in _mapList)
        {
            GameObject go = null;
            yield return _poolManager.GetObject(map, inst => go = inst, _poolManager.MapPool);
            if (go != null) _createdMaps.Add(go);
        }

        yield return _poolManager.GetObject(_bossMapRef, inst => _bossMap = inst, _poolManager.MapPool);

        if (_bossMap == null)
        {
            Debug.LogError("Boss map create fail");
        }

        _mapsReady = true;
        _isCreatingMaps = false;
    }

    public void ClearMap()
    {
        _mapList.Clear();
        _mapEnemyList.Clear();
        _bossEnemyList.Clear();

        _poolManager.ReturnObject(CurrentMap);
        CurrentMap = null;

        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
            _handle = default;
            _mapDict.Clear();
        }
    }
    #endregion

    #region Player

    public void PositionPlayer()
    {
        StartCoroutine(PositionPlayerCoroutine());
    }

    IEnumerator PositionPlayerCoroutine() 
    {
        yield return EnsureMapsReady();

        if (CurrentMap == null)
        {
            Debug.LogError("currentMap is null, map creation must precede");
            yield break;
        }

        Map map = CurrentMap.GetComponent<Map>();

        PlayerManager player = FindAnyObjectByType<PlayerManager>();

        player.transform.position = map.playerPoint.position;
    }

    #endregion

    #region Enemy
    public void SpawnEnemy(int count = 1, bool isBoss = false) 
    {
        StartCoroutine(SpawnEnemyCoroutine(count, isBoss));
    }

    private IEnumerator SpawnEnemyCoroutine(int count, bool isBoss)
    {
        // 1) 맵 준비 보장
        yield return EnsureMapsReady();

        if (CurrentMap == null) 
        {
            Debug.LogError("map is empty");
            yield break;
        }

        // 2) 풀에서 인스턴스 생성
        GameObject enemy = null;
        AssetReferenceGameObject enemyRef = null;
        List<Transform> spawnPoint = new List<Transform> (CurrentMap.GetComponent<Map>().SpawnPointOfEnemies);

        if (isBoss)
        {
            int bossIndex = StageManager.CurrentStageIndex / 10;
            enemyRef = _bossEnemyList[bossIndex];
        }

        for (int i = 0; i < count; i++)
        {
            if (!isBoss)
            {
                int randomIndex = UnityEngine.Random.Range(0, _mapEnemyList.Count);

                enemyRef = _mapEnemyList[randomIndex];
            }

            yield return _poolManager.GetObject(enemyRef, inst => enemy = inst, _poolManager.EnemyPool);

            int randomIndexOfSpawnPoint = UnityEngine.Random.Range(0, spawnPoint.Count);
            Transform sp = spawnPoint[randomIndexOfSpawnPoint];
            sp.gameObject.SetActive(true);
            enemy.transform.position = sp.position;

            spawnPoint.RemoveAt(randomIndexOfSpawnPoint);

            enemy.SetActive(true);
            if (enemy == null) yield break;
            EnemyManager.EnemySpawn(enemy, CurrentMapData, StageManager.CurrentStageIndex);
        }

        EnemyManager.AllEnemiesSpawned();
    }
    #endregion

    private void OnDestroy()
    {
        if (_handle.IsValid())
            Addressables.Release(_handle); // 수명 종료 시 일괄 해제
    }
}
