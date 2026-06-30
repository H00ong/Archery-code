using System;
using System.IO;
using Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Players;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    /// <summary>
    /// JSON 기반 저장 시스템. Newtonsoft.Json 직렬화. 위치: Application.persistentDataPath/save.json
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string FileName = "save.json";
        private const string BackupFileName = "save.json.bak";

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>가장 최근에 디스크에서 읽었거나 저장한 스냅샷. 없으면 null.</summary>
        public SaveData CurrentData { get; private set; }
        public bool HasLoadedData => CurrentData != null;

        public string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        public string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadFromDisk();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        // MapManager가 MapCleared로 MaxMapIndex를 먼저 갱신한 뒤 저장되도록 늦은 우선순위로 구독한다.
        private const int LateSavePriority = 100;

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.MapCleared, AutoSave, LateSavePriority);
            EventBus.Subscribe(EventType.PlayerDied, AutoSave, LateSavePriority);
            EventBus.Subscribe(EventType.TransitionToLobby, AutoSave);
            EventBus.Subscribe(EventType.CharacterPurchased, AutoSave);
            EventBus.Subscribe(EventType.CharacterLeveledUp, AutoSave);
            EventBus.Subscribe(EventType.CharacterSelected, AutoSave);
            EventBus.Subscribe(EventType.EquipmentPurchased, AutoSave);
            EventBus.Subscribe(EventType.EquipmentLeveledUp, AutoSave);
            EventBus.Subscribe(EventType.EquipmentChanged, AutoSave);
            EventBus.Subscribe(EventType.SettingsChanged, AutoSave);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.MapCleared, AutoSave);
            EventBus.Unsubscribe(EventType.PlayerDied, AutoSave);
            EventBus.Unsubscribe(EventType.TransitionToLobby, AutoSave);
            EventBus.Unsubscribe(EventType.CharacterPurchased, AutoSave);
            EventBus.Unsubscribe(EventType.CharacterLeveledUp, AutoSave);
            EventBus.Unsubscribe(EventType.CharacterSelected, AutoSave);
            EventBus.Unsubscribe(EventType.EquipmentPurchased, AutoSave);
            EventBus.Unsubscribe(EventType.EquipmentLeveledUp, AutoSave);
            EventBus.Unsubscribe(EventType.EquipmentChanged, AutoSave);
            EventBus.Unsubscribe(EventType.SettingsChanged, AutoSave);
        }

        private void OnApplicationQuit() => Save();

        private void OnApplicationPause(bool pause)
        {
            if (pause) Save();
        }

        public bool HasSave() => File.Exists(SavePath);

        private void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    CurrentData = null;
                    return;
                }

                string json = File.ReadAllText(SavePath);
                CurrentData = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
                if (CurrentData != null)
                    Debug.Log($"[SaveManager] Loaded save (v{CurrentData.saveVersion}) from {SavePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed: {ex.Message}. Trying backup...");
                TryRestoreFromBackup();
            }
        }

        private void TryRestoreFromBackup()
        {
            try
            {
                if (!File.Exists(BackupPath))
                {
                    CurrentData = null;
                    return;
                }
                string json = File.ReadAllText(BackupPath);
                CurrentData = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Backup restore failed: {ex.Message}");
                CurrentData = null;
            }
        }

        public void Save()
        {
            try
            {
                var data = BuildSnapshot();
                data.lastSavedAt = DateTime.UtcNow.ToString("o");

                if (File.Exists(SavePath))
                    File.Copy(SavePath, BackupPath, true);

                string json = JsonConvert.SerializeObject(data, JsonSettings);
                File.WriteAllText(SavePath, json);

                CurrentData = data;
                EventBus.Publish(EventType.GameSaved);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save failed: {ex.Message}");
            }
        }

        private void AutoSave() => Save();

        /// <summary>저장 파일을 모두 삭제하고 메모리 상태도 비운다. 초기화 후 씬 재로드는 호출자가 책임진다.</summary>
        public void ResetSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] ResetSave delete failed: {ex.Message}");
            }
            CurrentData = null;
            Debug.Log("[SaveManager] Save data reset.");
        }

        /// <summary>
        /// 저장 파일을 지우고, DontDestroyOnLoad 로 유지되는 모든 매니저를 제거한 뒤
        /// 부트(Loading) 씬부터 완전히 새로 시작한다. 모든 진행상황(골드/맵/캐릭터 등)이
        /// 빈 세이브 기준으로 재초기화된다.
        /// </summary>
        public void ResetAndRestart(string bootSceneName = "Loading")
        {
            ResetSave();
            Time.timeScale = 1f;

            // 영구 매니저들이 들고 있는 메모리 상태까지 비우기 위해
            // DontDestroyOnLoad 씬의 모든 루트 오브젝트를 제거한다.
            var probe = new GameObject("~ResetProbe");
            DontDestroyOnLoad(probe);
            var persistentScene = probe.scene;
            Destroy(probe);

            foreach (var root in persistentScene.GetRootGameObjects())
                Destroy(root);

            SceneManager.LoadScene(bootSceneName);
        }

        /// <summary>첫 실행 흐름(인트로/튜토리얼)이 끝났음을 표시.</summary>
        public void MarkFirstLaunchDone()
        {
            CurrentData ??= new SaveData();
            CurrentData.isFirstLaunch = false;
            Save();
        }

        private SaveData BuildSnapshot()
        {
            var data = new SaveData
            {
                saveVersion = SaveData.CurrentVersion,
                isFirstLaunch = CurrentData?.isFirstLaunch ?? true,
            };

            var pm = PlayerManager.Instance;
            if (pm != null && pm.PlayerData != null)
            {
                var pd = pm.PlayerData;
                data.gold = pd.gold;
                data.currentCharacterName = pd.currentCharacterName;

                foreach (var kv in pd.GetAllCharacterLevels())
                    data.characterLevels[kv.Key] = kv.Value;

                foreach (var name in pd.GetOwnedEquipments())
                    data.ownedEquipments.Add(name);

                foreach (var kv in pd.GetEquippedItems())
                    data.equippedItems[kv.Key] = kv.Value;

                foreach (var kv in pd.GetAllEquipmentTypeLevels())
                    data.equipmentTypeLevels[kv.Key] = kv.Value;
            }

            var cm = CharacterManager.Instance;
            if (cm != null)
            {
                foreach (var n in cm.GetOwnedCharacters())
                    data.ownedCharacters.Add(n);
            }

            var mm = MapManager.Instance;
            if (mm != null)
                data.maxMapIndex = mm.MaxMapIndex;

            var sm = SoundManager.Instance;
            if (sm != null)
            {
                data.bgmVolume = sm.BgmVolume;
                data.sfxVolume = sm.SfxVolume;
            }

            return data;
        }
    }
}
