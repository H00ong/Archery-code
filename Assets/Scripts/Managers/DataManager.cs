using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<string, EnemyData> enemyDataDict;

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

    private void MakeEnemyDict()
    {
        if (enemyDataDict != null)
            return;

        TextAsset jsonFile = Resources.Load<TextAsset>("Data/enemyData");
        string jsonText = "{\"enemies\":" + jsonFile.text + "}";  // ¹è¿­ °¨½Î±â

        EnemyDataList dataList = JsonUtility.FromJson<EnemyDataList>(jsonText);

        enemyDataDict = new Dictionary<string, EnemyData>();
        foreach (var data in dataList.enemies)
        {
            enemyDataDict[data.enemyName] = data;
        }
    }

    public EnemyData GetEnemyData(string enemyName)
    {
        MakeEnemyDict();

        if (enemyDataDict.TryGetValue(enemyName, out var data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning($"No data found for enemy: {enemyName}");
            return null;
        }
    }
}
