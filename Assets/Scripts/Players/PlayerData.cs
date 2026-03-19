using UnityEngine;

namespace Players
{
    public class PlayerData
    {
        public string characterName { get; private set; } = "BlueWizard";

        public void SetCharacterName(string characterName)
        {
            this.characterName = characterName;
        }

        public PlayerData(string characterName)
        {
            this.characterName = characterName;
        }
    }
}
