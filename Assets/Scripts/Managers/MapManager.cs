using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapManager : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] DataManager dataManager;
    [SerializeField] PoolManager poolManager;

    [Header("Required Objects")]
    [SerializeField] Transform mapParent;
    [SerializeField] Transform enemyPool;

    [Header("Default Postion")]
    [SerializeField] Vector3 defaultMapPosition;

    public static MapData currentMapData = null;

    public static int currentMapIndex = 0;
    public static MapType currentMapType;
    public static GameObject currentMap;
    private List<GameObject> createdMaps;

    [SerializeField] private string label = "map_config"; // Addressables 공통 라벨
    public static Dictionary<MapType, MapScriptable> mapDict = new Dictionary<MapType, MapScriptable>();
    private AsyncOperationHandle<IList<MapScriptable>> _handle; // 핸들 유지(언로드 방지)

    private List<AssetReferenceGameObject> mapList;
    private List<AssetReferenceGameObject> mapEnemyList;
    private List<AssetReferenceGameObject> bossEnemyList;
    private AssetReferenceGameObject bossMapRef;
    private GameObject bossMap;

    private bool _mapsReady = false;
    private bool _isCreatingMaps = false;

    // 순서 보장: 라벨 로드 완료 후 데이터 읽기/맵 생성

    // Addressables 라벨로 MapScriptable 일괄 로드 → dict 구성

    #region Map
    private IEnumerator EnsureMapDictReady()
    {
        if (mapDict != null && mapDict.Count > 0)
            yield break;

        if (mapDict == null)
            mapDict = new Dictionary<MapType, MapScriptable>();

        _handle = Addressables.LoadAssetsAsync<MapScriptable>(
            label,
            so => mapDict[so.mapType] = so // 동일 key 존재 시 마지막 항목으로 갱신
        );
        yield return _handle;

        if (_handle.Status != AsyncOperationStatus.Succeeded)
            Debug.LogError($"[MapManager] Addressables Load failed. label={label}");
    }

    private void GetMapDataFromDataManager()
    {
        if (currentMapData != null) return;

        if (dataManager == null)
            dataManager = FindAnyObjectByType<DataManager>();

        if (dataManager == null)
        {
            Debug.LogError("data manager is null");
            return;
        }

        currentMapData = dataManager.GetMapData(currentMapIndex);
        if (currentMapData == null) return;

        currentMapType = (MapType)currentMapData.mapType;
        if (!mapDict.TryGetValue(currentMapType, out var data))
        {
            Debug.LogError($"[MapManager] MapScriptable not found for {currentMapType}");
            return;
        }

        // 참조 복사 대신 새 리스트로 교체(외부에서 Clear()해도 원본 SO 영향 없음)
        mapList = new List<AssetReferenceGameObject>(data.mapList);
        mapEnemyList = new List<AssetReferenceGameObject>(data.enemyList);
        bossEnemyList = new List<AssetReferenceGameObject>(data.bossList);
        bossMapRef = data.bossMap;
    }

    public void GetNewMap(bool isBossMap = false)
    {
        StartCoroutine(GetMapCoroutine(go =>
        {
            if (go == null) return;
            else
            {
                if (currentMap != null)
                    currentMap.SetActive(false); // 기존 맵 비활성화

                currentMap = go;
                currentMap.GetComponent<Map>().Init();
                currentMap.transform.position = defaultMapPosition;
                currentMap.SetActive(true);
            }
        }, isBossMap));
    }

    public IEnumerator GetMapCoroutine(Action<GameObject> onReady, bool isBossMap = false)
    {
        yield return EnsureMapsReady();

        if (!isBossMap)
        {
            if (createdMaps == null || createdMaps.Count == 0)
            {
                Debug.LogError("No maps created.");
                onReady?.Invoke(null);
                yield break;
            }

            int idx = UnityEngine.Random.Range(0, createdMaps.Count);
            onReady?.Invoke(createdMaps[idx]);
        }
        else
        {
            if (bossMap == null)
            {
                Debug.LogError("Boss maps not created");
                onReady?.Invoke(null);
                yield break;
            }

            onReady?.Invoke(bossMap);
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

        if (createdMaps != null) createdMaps.Clear();
        else createdMaps = new List<GameObject>();

        if (poolManager == null)
            poolManager = FindAnyObjectByType<PoolManager>();

        if (poolManager == null)
        {
            Debug.LogError("pool manager is null");
            yield break;
        }

        foreach (var map in mapList)
        {
            GameObject go = null;
            yield return poolManager.GetObject(map, inst => go = inst, mapParent);
            if (go != null) createdMaps.Add(go);
        }

        yield return poolManager.GetObject(bossMapRef, inst => bossMap = inst, mapParent);
        if (bossMap == null)
        {
            Debug.LogError("Boss map create fail");
        }

        _mapsReady = true;
        _isCreatingMaps = false;
    }

    public void ClearMap()
    {
        mapList.Clear();
        mapEnemyList.Clear();
        bossEnemyList.Clear();

        poolManager.ReturnObject(currentMap);
        currentMap = null;

        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
            _handle = default;
            mapDict.Clear();
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

        if (currentMap == null)
        {
            Debug.LogError("currentMap is null, map creation must precede");
            yield break;
        }

        Map map = currentMap.GetComponent<Map>();

        PlayerManager player = FindAnyObjectByType<PlayerManager>();

        player.transform.position = map.playerPoint.position;
    }

    #endregion

    #region Enemy
    public void SpawnEnemy(int count = 1, bool isBoss = false) 
    {
        StartCoroutine(SpawnEnemyAsync(count, isBoss));
    }

    private IEnumerator SpawnEnemyAsync(int count, bool isBoss)
    {
        // 1) 맵 준비 보장
        yield return EnsureMapsReady();

        if (currentMap == null) 
        {
            Debug.LogError("map is empty");
            yield break;
        }

        // 2) 풀에서 인스턴스 생성
        GameObject enemy = null;
        AssetReferenceGameObject enemyRef = null;
        List<Transform> spawnPoint = new List<Transform> (currentMap.GetComponent<Map>().spawnPointOfEnemies);

        if (isBoss)
        {
            int bossIndex = (StageManager.currentStageIndex - 1) / 10;
            enemyRef = bossEnemyList[bossIndex];
        }

        for (int i = 0; i < count; i++)
        {
            if (!isBoss)
            {
                int randomIndex = UnityEngine.Random.Range(0, mapEnemyList.Count);

                enemyRef = mapEnemyList[randomIndex];
            }

            yield return poolManager.GetObject(enemyRef, inst => enemy = inst, enemyPool, extraPrewarmCount: 0);

            int randomIndexOfSpawnPoint = UnityEngine.Random.Range(0, spawnPoint.Count);
            Transform sp = spawnPoint[randomIndexOfSpawnPoint];
            sp.gameObject.SetActive(true);
            enemy.transform.position = sp.position;

            spawnPoint.RemoveAt(randomIndexOfSpawnPoint);

            enemy.SetActive(true);

            if (enemy == null) yield break;

            EnemyManager.EnemySpawn(enemy, currentMapData, currentMapIndex);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (_handle.IsValid())
            Addressables.Release(_handle); // 수명 종료 시 일괄 해제
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();
        if (!dataManager) dataManager = FindAnyObjectByType<DataManager>();
    }
#endif
}
