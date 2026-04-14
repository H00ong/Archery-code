using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class PlayerData
    {
        public string currentCharacterName { get; private set; }
        public int gold { get; private set; }

        private readonly Dictionary<string, int> _characterLevels = new();

        public void SetCharacterName(string characterName)
        {
            currentCharacterName = characterName;
        }

        public PlayerData(string characterName, int startGold = 500)
        {
            currentCharacterName = characterName;
            gold = startGold;
            _characterLevels[characterName] = 1; // Initialize the starting character level
        }

        public PlayerData(string savedCharacterName, Dictionary<string, int> savedCharacterLevels, int savedGold = 500)
        {
            currentCharacterName = savedCharacterName;
            gold = savedGold;
            _characterLevels = savedCharacterLevels ?? new Dictionary<string, int>();
        }

        public int GetCharacterLevel(string name)
        {
            return _characterLevels.TryGetValue(name, out int lv) ? lv : 1;
        }

        public void SetCharacterLevel(string name, int level)
        {
            _characterLevels[name] = level;
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || gold < amount)
                return false;
            gold -= amount;
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount > 0)
                gold += amount;
        }
    }
}
