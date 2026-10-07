using Players;
using UnityEngine;

namespace Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        public PlayerData PlayerData { get; private set; }

        /// <summary> 이번 판(런)에서 획득한 골드 누적치. 새로운 판이 시작될 때 0으로 초기화된다. </summary>
        public int RunGold { get; private set; }

        // 결과(클리어/게임오버)에서 영구 골드로 이미 반영했는지 여부. 중복 반영 방지.
        private bool _runGoldCommitted;

        // SaveManager(100)보다 먼저, UI(0) 이후에 실행되도록 하여 저장 시 반영된 골드가 기록되게 한다.
        private const int CommitPriority = 50;

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
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.PlayerSpawned, ResetRunGold);
            EventBus.Subscribe(EventType.PlayerDied, CommitRunGold, CommitPriority);
            EventBus.Subscribe(EventType.MapCleared, CommitRunGold, CommitPriority);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.PlayerSpawned, ResetRunGold);
            EventBus.Unsubscribe(EventType.PlayerDied, CommitRunGold);
            EventBus.Unsubscribe(EventType.MapCleared, CommitRunGold);
        }

        /// <summary> 인게임에서 골드 획득. 이번 판 누적치(RunGold)에만 쌓고, 영구 골드는 결과 시점에 반영한다. </summary>
        public void EarnGold(int amount)
        {
            if (amount <= 0) return;

            RunGold += amount;
        }

        private void CommitRunGold()
        {
            if (_runGoldCommitted) return;

            _runGoldCommitted = true;
            PlayerData?.AddGold(RunGold);
        }

        private void ResetRunGold()
        {
            RunGold = 0;
            _runGoldCommitted = false;
        }

        public void InitializePlayerData()
        {
            PlayerData = DataManager.Instance.GetPlayerData();
        }

        public void SyncPlayerData(PlayerData playerData)
        {
            PlayerData = playerData;

            var characterManager = CharacterManager.Instance;
            characterManager.SyncCharacterIdentity(playerData.currentCharacterName);
        }

        public void SetCurrentCharacter(CharacterIdentity characterIdentity)
        {
            PlayerData.SetCharacterName(characterIdentity.characterName);
            SyncPlayerData(PlayerData);
            EventBus.Publish(EventType.CharacterSelected);
        }
    }
}

