using UnityEngine;

namespace Players
{
    public class PlayerData
    {
        public CharacterName characterName { get; private set; } = CharacterName.BlueWizard;

        public void SetCharacterName(CharacterName characterName)
        {
            this.characterName = characterName;
        }

        public PlayerData(CharacterName characterName)
        {
            this.characterName = characterName;
        }
    }
}
