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
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.PlayerSpawned, ResetRunGold);
        }

        /// <summary> 인게임에서 골드 획득. 영구 골드(PlayerData)와 이번 판 누적치(RunGold)에 모두 반영한다. </summary>
        public void EarnGold(int amount)
        {
            if (amount <= 0) return;

            PlayerData?.AddGold(amount);
            RunGold += amount;
        }

        private void ResetRunGold()
        {
            RunGold = 0;
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

