using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class PlayerData
    {
        public string currentCharacterName { get; private set; }
        public int gold { get; private set; }

        private readonly Dictionary<string, int> _characterLevels = new();

        // ── Equipment ──
        // 소유한 장비 목록
        private readonly HashSet<string> _ownedEquipments = new();
        // 현재 장착 중인 장비 (종류별 이름)
        private readonly Dictionary<EquipmentType, string> _equippedItems = new();
        // 장비 종류별 공유 레벨
        private readonly Dictionary<EquipmentType, int> _equipmentTypeLevels = new();

        public void SetCharacterName(string characterName)
        {
            currentCharacterName = characterName;
        }

        public PlayerData(string characterName, string weaponName, string armorName, string shoesName, int startGold = 500)
        {
            currentCharacterName = characterName;
            gold = startGold;
            _characterLevels[characterName] = 1;

            // 초기 장비 설정 (소유 + 장착)
            _ownedEquipments.Add(weaponName);
            _ownedEquipments.Add(armorName);
            _ownedEquipments.Add(shoesName);
            _equippedItems[EquipmentType.Weapon] = weaponName;
            _equippedItems[EquipmentType.Armor] = armorName;
            _equippedItems[EquipmentType.Shoes] = shoesName;

            // 장비 종류별 초기 레벨
            _equipmentTypeLevels[EquipmentType.Weapon] = 1;
            _equipmentTypeLevels[EquipmentType.Armor] = 1;
            _equipmentTypeLevels[EquipmentType.Shoes] = 1;
        }

        public PlayerData(string savedCharacterName, Dictionary<string, int> savedCharacterLevels,
            HashSet<string> savedOwnedEquipments,
            Dictionary<EquipmentType, string> savedEquippedItems,
            Dictionary<EquipmentType, int> savedEquipmentTypeLevels,
            int savedGold = 500)
        {
            currentCharacterName = savedCharacterName;
            gold = savedGold;
            _characterLevels = savedCharacterLevels ?? new Dictionary<string, int>();
            _ownedEquipments = savedOwnedEquipments ?? new HashSet<string>();
            _equippedItems = savedEquippedItems ?? new Dictionary<EquipmentType, string>();
            _equipmentTypeLevels = savedEquipmentTypeLevels ?? new Dictionary<EquipmentType, int>();
        }

        // ── Character Level ──

        public int GetCharacterLevel(string name)
        {
            return _characterLevels.TryGetValue(name, out int lv) ? lv : 1;
        }

        public void SetCharacterLevel(string name, int level)
        {
            _characterLevels[name] = level;
        }

        // ── Equipment ──

        public string GetEquippedItemName(EquipmentType type)
        {
            return _equippedItems.TryGetValue(type, out var name) ? name : null;
        }

        public void SetEquippedItem(EquipmentType type, string equipmentName)
        {
            _equippedItems[type] = equipmentName;
        }

        public IReadOnlyDictionary<EquipmentType, string> GetEquippedItems() => _equippedItems;

        public bool IsEquipmentOwned(string equipmentName) => _ownedEquipments.Contains(equipmentName);

        public bool AddOwnedEquipment(string equipmentName) => _ownedEquipments.Add(equipmentName);

        public IReadOnlyCollection<string> GetOwnedEquipments() => _ownedEquipments;

        public int GetEquipmentTypeLevel(EquipmentType type)
        {
            return _equipmentTypeLevels.TryGetValue(type, out int lv) ? lv : 1;
        }

        public void SetEquipmentTypeLevel(EquipmentType type, int level)
        {
            _equipmentTypeLevels[type] = level;
        }

        // ── Gold ──

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
