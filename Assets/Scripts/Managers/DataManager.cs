using System;
using System.Collections.Generic;
using Enemy;
using Map;
using Newtonsoft.Json;
using Players;
using UnityEngine;

namespace Managers
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }
        [SerializeField] private TextAsset enemyDataJson;
        [SerializeField] private TextAsset mapDataJson;
        private Dictionary<EnemyKey, EnemyData> enemyDataDict;
        private List<MapData> mapDataList;
        private PlayerData playerData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        #region Init

        public void Init()
        {
            LoadPlayerData();
            LoadEnemyData();
            LoadMapData();
        }

        #endregion

        #region Enemy

        private void LoadEnemyData()
        {
            if (enemyDataDict != null)
                return;

            try
            {
                TextAsset json = enemyDataJson;
                if (!json)
                    throw new InvalidOperationException("enemyDataJson is not assigned in Inspector");

                string text = json.text;

                var enemies = JsonConvert.DeserializeObject<List<EnemyData>>(text);
                if (enemies == null || enemies.Count == 0)
                    throw new InvalidOperationException("EnemyData parse failed or empty");

                enemyDataDict = new Dictionary<EnemyKey, EnemyData>();

                foreach (var enemyData in enemies)
                {
                    enemyData.enemyTags ??= new List<EnemyTag>();

                    EnemyTag mask = EnemyTag.None;
                    foreach (var tag in enemyData.enemyTags)
                        mask |= tag;

                    var key = new EnemyKey { Name = enemyData.enemyName, Tag = mask };

                    if (enemyDataDict.TryAdd(key, enemyData))
                        continue;
                    Debug.LogError($"Duplicate enemy key: {enemyData.enemyName} | mask={mask}");
                }

                Debug.Log($"Enemy loaded: {enemyDataDict.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataManager] LoadEnemyData failed: {ex.Message}");
                throw;
            }
        }

        public void ClearEnemyData()
        {
            enemyDataDict.Clear();
            enemyDataDict = null;
        }

        public EnemyData GetEnemyData(string enemyName, EnemyTag enemyTag)
        {
            var key = new EnemyKey { Name = enemyName, Tag = enemyTag };

            if (enemyDataDict.TryGetValue(key, out var data))
                return data;

            Debug.LogWarning($"No data found for enemy: {enemyName} with tag {enemyTag}");
            return null;
        }
        #endregion

        #region Map

        private void LoadMapData()
        {
            try
            {
                TextAsset jsonFile = mapDataJson;
                if (!jsonFile)
                    throw new InvalidOperationException("mapDataJson is not assigned in Inspector");

                var maps = JsonConvert.DeserializeObject<List<MapData>>(jsonFile.text);
                if (maps == null || maps.Count == 0)
                    throw new InvalidOperationException("MapData parse failed or empty");

                mapDataList = maps;

                Debug.Log($"Loaded {mapDataList.Count} maps.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataManager] LoadMapData failed: {ex.Message}");
                throw;
            }
        }

        public MapData GetMapData(int index)
        {
            if (index >= 0 && mapDataList != null && index < mapDataList.Count)
                return mapDataList[index];

            Debug.LogWarning($"Invalid map index {index}");
            return null;
        }

        public int GetMapCount() => mapDataList?.Count ?? 0;

        public void ClearMapData() => mapDataList.Clear();

        #endregion

        #region Player
        public void LoadPlayerData()
        {
            // TODO : Load PlayerData from SaveSystem
            playerData = new PlayerData("BlueWizard", "Normal Magic Wand", "Old Armor", "Old Shoes", 5000);
        }

        public PlayerData GetPlayerData()
        {
            return playerData;
        }
        #endregion
    }
}

