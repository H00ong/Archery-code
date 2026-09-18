using System.Collections.Generic;
using Players;

namespace SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;
        public const string DefaultCharacterName = "BlueWizard";

        public int saveVersion = CurrentVersion;
        public string lastSavedAt;

        // Progression
        public int gold;
        public int maxMapIndex = -1;

        // Character
        public string currentCharacterName;
        public List<string> ownedCharacters = new();
        public Dictionary<string, int> characterLevels = new();

        // Equipment
        public List<string> ownedEquipments = new();
        public Dictionary<EquipmentType, string> equippedItems = new();
        public Dictionary<EquipmentType, int> equipmentTypeLevels = new();

        // Settings
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;

        // Meta
        public bool isFirstLaunch = true;

        public void EnsureDefaultCharacter()
        {
            ownedCharacters ??= new List<string>();
            characterLevels ??= new Dictionary<string, int>();

            if (!ownedCharacters.Contains(DefaultCharacterName))
                ownedCharacters.Add(DefaultCharacterName);

            if (!characterLevels.ContainsKey(DefaultCharacterName))
                characterLevels[DefaultCharacterName] = 1;

            if (!string.IsNullOrEmpty(currentCharacterName) && !ownedCharacters.Contains(currentCharacterName))
                ownedCharacters.Add(currentCharacterName);
        }
    }
}
