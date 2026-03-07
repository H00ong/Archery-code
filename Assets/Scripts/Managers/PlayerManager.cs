using Players;
using UnityEngine;

namespace Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        public PlayerData PlayerData { get; private set; }

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

        public void InitializePlayerData()
        {
            PlayerData = DataManager.Instance.GetPlayerData();
        }

        public void SyncPlayerData(PlayerData playerData)
        {
            PlayerData = playerData;

            var characterManager = CharacterManager.Instance;
            characterManager.SyncCharacterIdentity(playerData.characterName);
        }

        public void SetCurrentCharacter(CharacterIdentity characterIdentity)
        {
            PlayerData.SetCharacterName(characterIdentity.characterName);
            SyncPlayerData(PlayerData);
        }
    }
}

