using System.Collections.Generic;
using Players;
using Stat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        [Header("Addressable Settings")]
        [SerializeField] private string equipmentLabel = "equipment_identity";

        private AsyncOperationHandle<IList<EquipmentIdentity>> _loadHandle;

        private readonly Dictionary<string, EquipmentIdentity> _equipmentMap = new();

        // 현재 장착 중인 장비 (타입별 1개)
        private readonly Dictionary<EquipmentType, EquipmentIdentity> _equippedItems = new();

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

        private void OnDisable()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }

        public async Awaitable LoadEquipmentIdentitiesAsync()
        {
            _loadHandle = Addressables.LoadAssetsAsync<EquipmentIdentity>(equipmentLabel, null);
            await _loadHandle.Task;
            destroyCancellationToken.ThrowIfCancellationRequested();

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _equipmentMap.Clear();

                foreach (var so in _loadHandle.Result)
                {
                    if (so && !_equipmentMap.TryAdd(so.equipmentName, so))
                    {
                        Debug.LogWarning($"[EquipmentManager] Duplicate EquipmentName: {so.equipmentName}");
                    }
                }
                Debug.Log($"[EquipmentManager] Loaded {_equipmentMap.Count} equipment identities.");

                // 현재 장착 장비 복원
                var playerData = PlayerManager.Instance.PlayerData;
                foreach (var kvp in playerData.GetEquippedItems())
                {
                    if (!string.IsNullOrEmpty(kvp.Value) && _equipmentMap.TryGetValue(kvp.Value, out var identity))
                    {
                        _equippedItems[kvp.Key] = identity;
                    }
                }
            }
            else
            {
                Debug.LogError("[EquipmentManager] Failed to load equipment identities.");
                throw new System.InvalidOperationException("Failed to load equipment identities.");
            }
        }

        /// <summary>
        /// 장비를 교체한다. 맵만 갱신하며, 스탯 재계산은 LoadAndSpawnCharacterAsync에서 수행된다.
        /// </summary>
        public void EquipItem(string equipmentName)
        {
            if (!_equipmentMap.TryGetValue(equipmentName, out var newIdentity))
            {
                Debug.LogError($"[EquipmentManager] Equipment not found: {equipmentName}");
                return;
            }

            var type = newIdentity.equipmentType;
            var playerData = PlayerManager.Instance.PlayerData;

            _equippedItems[type] = newIdentity;
            playerData.SetEquippedItem(type, equipmentName);

            Debug.Log($"[EquipmentManager] Equipped {equipmentName} (Type: {type})");
            EventBus.Publish(EventType.EquipmentChanged);
        }

        /// <summary>
        /// 현재 장착한 모든 장비의 스탯을 PlayerStat에 적용한다 (게임 시작 시).
        /// Equipment Layer를 전부 초기화한 뒤 3개 장비의 스탯 + EffectData를 합산한다.
        /// </summary>
        public void ApplyAllEquipmentStats(PlayerStat playerStat)
        {
            // Equipment Layer 전체 초기화
            playerStat.ResetEquipmentStats();

            var playerData = PlayerManager.Instance.PlayerData;

            foreach (var kvp in _equippedItems)
            {
                int level = playerData.GetEquipmentTypeLevel(kvp.Key);
                var stats = kvp.Value.GetStatsAtLevel(level);

                playerStat.SetEquipMaxHP(playerStat.GetEquipMaxHP() + stats.maxHP);
                playerStat.SetEquipAttackPower(playerStat.GetEquipAttackPower() + stats.attackPower);
                playerStat.SetEquipMoveSpeed(playerStat.GetEquipMoveSpeed() + stats.moveSpeed);
                playerStat.SetEquipArmor(playerStat.GetEquipArmor() + stats.armor);
                playerStat.SetEquipMagicResistance(playerStat.GetEquipMagicResistance() + stats.magicResistance);
                playerStat.SetEquipAttackSpeed(playerStat.GetEquipAttackSpeed() + stats.attackSpeed);
                playerStat.SetEquipProjectileSpeed(playerStat.GetEquipProjectileSpeed() + stats.projectileSpeed);

                if (stats.attackEffectType != EffectType.Normal)
                    playerStat.SetEquipAttackEffectType(stats.attackEffectType);
            }

            RecalculateEquipEffectData(playerStat);
        }

        /// <summary>
        /// 특정 장비 종류의 현재 스탯을 반환한다. 장착 중인 장비가 없으면 default를 반환한다.
        /// </summary>
        public BaseStatData GetStatOfEquipment(EquipmentType type)
        {
            if (!_equippedItems.TryGetValue(type, out var identity))
                return default;

            int level = PlayerManager.Instance.PlayerData.GetEquipmentTypeLevel(type);
            return identity.GetStatsAtLevel(level);
        }

        /// <summary>
        /// 3개 장비의 EffectData를 합산하여 PlayerStat의 Equipment EffectData에 설정한다.
        /// </summary>
        private void RecalculateEquipEffectData(PlayerStat playerStat)
        {
            playerStat.ClearEquipEffectData();

            var playerData = PlayerManager.Instance.PlayerData;
            var merged = new Dictionary<EffectType, EffectData>();
            EffectType combinedEffectType = EffectType.Normal;

            foreach (var kvp in _equippedItems)
            {
                int level = playerData.GetEquipmentTypeLevel(kvp.Key);
                var identity = kvp.Value;

                var effectMap = identity.GetEffectDataAtLevel(level);
                foreach (var effectKvp in effectMap)
                {
                    if (merged.TryGetValue(effectKvp.Key, out var existing))
                        merged[effectKvp.Key] = existing + effectKvp.Value;
                    else
                        merged[effectKvp.Key] = effectKvp.Value.Clone();
                }

                var stats = identity.GetStatsAtLevel(level);
                combinedEffectType |= stats.attackEffectType;
            }

            // 합산된 EffectData를 PlayerStat에 설정
            foreach (var kvp in merged)
                playerStat.SetEquipEffectData(kvp.Key, kvp.Value);

            // 합산된 EffectType 설정 (기존 값 초기화 후 재설정)
            playerStat.ResetEquipAttackEffectType();
            playerStat.SetEquipAttackEffectType(combinedEffectType);
        }

        public EquipmentIdentity GetEquippedItem(EquipmentType type)
        {
            _equippedItems.TryGetValue(type, out var identity);
            return identity;
        }

        public EquipmentIdentity GetEquipmentByName(string equipmentName)
        {
            _equipmentMap.TryGetValue(equipmentName, out var identity);
            return identity;
        }

        public IReadOnlyDictionary<string, EquipmentIdentity> GetEquipmentMap() => _equipmentMap;

        public bool IsEquipmentUnlocked(string equipmentName) => PlayerManager.Instance.PlayerData.IsEquipmentOwned(equipmentName);

        public void UnlockEquipment(string equipmentName)
        {
            if (PlayerManager.Instance.PlayerData.AddOwnedEquipment(equipmentName))
            {
                Debug.Log($"[EquipmentManager] Equipment unlocked: {equipmentName}");
            }
        }

        public bool TryLevelUpEquipmentType(EquipmentType type)
        {
            // 장비 종류별 레벨은 공유 — 현재 장착 중인 장비의 maxLevel을 기준으로 사용
            if (!_equippedItems.TryGetValue(type, out var identity))
                return false;

            var playerData = PlayerManager.Instance.PlayerData;
            int currentLevel = playerData.GetEquipmentTypeLevel(type);

            if (currentLevel >= identity.maxLevel)
                return false;

            int cost = identity.GetLevelUpCost(currentLevel);
            if (cost < 0 || !playerData.SpendGold(cost))
                return false;

            playerData.SetEquipmentTypeLevel(type, currentLevel + 1);
            Debug.Log($"[EquipmentManager] '{type}' 레벨업: {currentLevel} → {currentLevel + 1}");
            EventBus.Publish(EventType.EquipmentLeveledUp);
            return true;
        }

        public bool TryPurchaseEquipment(string equipmentName)
        {
            if (!_equipmentMap.TryGetValue(equipmentName, out var identity))
                return false;

            if (PlayerManager.Instance.PlayerData.IsEquipmentOwned(equipmentName))
                return false;

            var playerData = PlayerManager.Instance.PlayerData;

            if (!playerData.SpendGold(identity.purchasePrice))
                return false;

            UnlockEquipment(equipmentName);
            EventBus.Publish(EventType.EquipmentPurchased);
            return true;
        }
    }
}
