using Game.Enemies;
using Game.Enemies.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public static Dictionary<EnemyKey, EnemyData> enemyDataDict;
    public static List<MapData> mapDataList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    #region Enemy
    private void LoadEnemyData()
    {
        if (enemyDataDict != null) return;

        TextAsset json = Resources.Load<TextAsset>("Data/enemyData");
        if (json == null)
        {
            Debug.LogError("Resources/Data/enemyData.json not found");
            return;
        }

        // 루트가 배열([])이면 래핑, {"enemies":[...]}이면 그대로
        string text = json.text.TrimStart();
        string wrapped = text.StartsWith("[") ? "{\"enemies\":" + json.text + "}" : json.text;

        var wrapper = JsonUtility.FromJson<EnemyDataWrapper>(wrapped);
        if (wrapper?.enemies == null || wrapper.enemies.Count == 0)
        {
            Debug.LogError("EnemyData parse failed or empty");
            return;
        }

        enemyDataDict = new Dictionary<EnemyKey, EnemyData>();

        foreach (var enemy in wrapper.enemies)
        {
            // tags null 보호
            enemy.tags ??= new List<string>();

            // 문자열 태그 → 비트마스크
            var mask = Game.Enemies.EnemyTagUtil.ParseTagsToMask(enemy.tags);
            EnemyName enemyName = EnemyName.None;

            if (Enum.TryParse(enemy.enemyName, ignoreCase: true, out EnemyName name))
                enemyName = name;
            else
            {
                Debug.LogError("EnemyData name parsing failed");
                continue;
            }

            var key = new Game.Enemies.EnemyKey
            {
                Name = enemyName,
                Tag = mask
            };

            if (enemyDataDict.ContainsKey(key))
            {
                Debug.LogError($"Duplicate enemy key: {enemy.enemyName} | mask={mask}");
                continue; // 또는 덮어쓰기 허용 시: enemyDataDict[key] = enemy;
            }

            enemyDataDict.Add(key, enemy);
        }

        Debug.Log($"Enemy loaded: {enemyDataDict.Count}");
    }

    public void ClearEnemyData()
    {
        if (enemyDataDict == null) return;
        enemyDataDict.Clear();
        enemyDataDict = null;
    }

    public EnemyData GetEnemyData(EnemyName name, EnemyTag tag)
    {
        LoadEnemyData();
        var key = new EnemyKey { Name = name, Tag = tag };

        if (enemyDataDict.TryGetValue(key, out var data))
            return data;

        Debug.LogWarning($"No data found for enemy: {name} with tag {tag}");
        return null;
    }
    #endregion

    #region Map

    private void LoadMapData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/mapData");
        if (jsonFile == null)
        {
            Debug.LogError("Map data file not found at Resources/Data/mapData.json");
            return;
        }

        // Wrapper로 먼저 역직렬화
        MapDataWrapper wrapper = JsonUtility.FromJson<MapDataWrapper>(jsonFile.text);
        mapDataList = wrapper.maps;

        Debug.Log($"Loaded {mapDataList.Count} maps.");
    }

    // 특정 맵 인덱스로 접근
    public MapData GetMapData(int index)
    {
        if (mapDataList == null)
            LoadMapData();

        if (index < 0 || index >= mapDataList.Count)
        {
            Debug.LogWarning($"Invalid map index {index}");
            return null;
        }

        return mapDataList[index];
    }

    public void ClearMapData() => mapDataList.Clear();

    #endregion
}
