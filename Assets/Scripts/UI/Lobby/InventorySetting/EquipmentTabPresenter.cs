using System.Collections.Generic;
using Managers;
using Players;
using Stat;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Inventory 하부 탭(Weapon/Armor/Shoes) 1개에 대응되는 Presenter.
    /// CharacterTabPresenter와 동일한 구조를 갖되, 특정 EquipmentType으로 필터링된 장비 목록을 순회한다.
    /// 장비 레벨은 종류별로 공유되며(레벨업은 착용 중인 장비에서만 가능), 데이터는 EquipmentManager를 사용한다.
    /// </summary>
    public class EquipmentTabPresenter
    {
        private readonly UI_EquipmentTabView _view;
        private readonly EquipmentType _equipmentType;
        private readonly EquipmentManager _equipmentManager;
        private readonly PlayerData _playerData;

        private readonly List<string> _equipmentNames = new();
        private int _currentIndex;
        private int _equippedIndex;

        private EquipmentIdentity _currentViewingIdentity;
        private EquipmentIdentity _currentEquippedIdentity;
        private int _currentViewingLevel;
        private int _currentEquippedLevel;

        public EquipmentTabPresenter(UI_EquipmentTabView view, EquipmentType equipmentType)
        {
            _view = view;
            _equipmentType = equipmentType;
            _equipmentManager = EquipmentManager.Instance;
            _playerData = PlayerManager.Instance.PlayerData;

            _view.Init(OnLeft, OnRight, OnActionButton, OnGrowthDetail, OnCurrentStatsDetail, OnEquippedStatsDetail);
        }

        public void Activate()
        {
            BuildEquipmentList();
            UpdateView();
            UpdateEquippedContents();
        }

        public void Deactivate()
        {
            _view.CloseAllDetailPopups();
            _currentIndex = _equippedIndex;
        }

        private void BuildEquipmentList()
        {
            _equipmentNames.Clear();

            var map = _equipmentManager.GetEquipmentMap();
            foreach (var kvp in map)
            {
                if (kvp.Value != null && kvp.Value.equipmentType == _equipmentType)
                    _equipmentNames.Add(kvp.Key);
            }

            // 정렬: index 기준 (CharacterIdentity와 동일 방식)
            _equipmentNames.Sort((a, b) =>
            {
                var idA = _equipmentManager.GetEquipmentByName(a);
                var idB = _equipmentManager.GetEquipmentByName(b);
                return idA.index.CompareTo(idB.index);
            });

            string equippedName = _playerData.GetEquippedItemName(_equipmentType);
            int eqIdx = _equipmentNames.IndexOf(equippedName);
            if (eqIdx < 0) eqIdx = 0;

            _equippedIndex = eqIdx;
            _currentIndex = eqIdx;
        }

        private void OnLeft()
        {
            if (_equipmentNames.Count == 0) return;
            int newIndex = Mathf.Clamp(_currentIndex - 1, 0, _equipmentNames.Count - 1);
            if (newIndex == _currentIndex) return;
            _currentIndex = newIndex;
            UpdateView();
        }

        private void OnRight()
        {
            if (_equipmentNames.Count == 0) return;
            int newIndex = Mathf.Clamp(_currentIndex + 1, 0, _equipmentNames.Count - 1);
            if (newIndex == _currentIndex) return;
            _currentIndex = newIndex;
            UpdateView();
        }

        private void OnCurrentStatsDetail()
        {
            if (_currentViewingIdentity == null) return;
            string detail = LobbyStatFormatter.FormatCurrentStatsDetail(
                _currentViewingIdentity.GetEffectDataAtLevel(_currentViewingLevel),
                _currentEquippedIdentity?.GetEffectDataAtLevel(_currentEquippedLevel));
            _view.ShowCurrentStatsDetailPopup(detail);
        }

        private void OnGrowthDetail()
        {
            if (_currentViewingIdentity == null) return;
            string detail = LobbyStatFormatter.FormatGrowthStatsDetail(
                _currentViewingIdentity.effectGrowths, _currentEquippedIdentity?.effectGrowths);
            _view.ShowGrowthDetailPopup(detail);
        }

        private void OnEquippedStatsDetail()
        {
            if (_currentEquippedIdentity == null) return;
            var effectMap = _currentEquippedIdentity.GetEffectDataAtLevel(_currentEquippedLevel);
            string detail = LobbyStatFormatter.FormatEquippedStatsDetail(effectMap);
            _view.ShowEquippedStatsDetailPopup(detail);
        }

        private void OnActionButton()
        {
            if (_currentViewingIdentity == null) return;

            string name = _currentViewingIdentity.equipmentName;
            bool unlocked = _equipmentManager.IsEquipmentUnlocked(name);
            bool equipped = IsCurrentEquipped();

            if (!unlocked)
            {
                if (_equipmentManager.TryPurchaseEquipment(name))
                    UpdateView();
            }
            else if (equipped)
            {
                if (_equipmentManager.TryLevelUpEquipmentType(_equipmentType))
                {
                    UpdateView();
                    UpdateEquippedContents();
                }
            }
            else
            {
                _equipmentManager.EquipItem(name);
                _equippedIndex = _currentIndex;
                UpdateView();
                UpdateEquippedContents();
            }
        }

        private bool IsCurrentEquipped()
        {
            if (_currentIndex < 0 || _currentIndex >= _equipmentNames.Count) return false;
            return _equipmentNames[_currentIndex] == _playerData.GetEquippedItemName(_equipmentType);
        }

        private void UpdateView()
        {
            if (_equipmentNames.Count == 0 || _currentIndex < 0) return;

            string name = _equipmentNames[_currentIndex];
            var identity = _equipmentManager.GetEquipmentByName(name);
            if (identity == null) return;

            bool unlocked = _equipmentManager.IsEquipmentUnlocked(name);
            bool locked = !unlocked;
            bool equipped = IsCurrentEquipped();

            _view.SetEquipmentIcon(identity.equipmentIcon);
            _view.SetEquipmentName(name);
            _view.SetLockIconActive(locked);
            _view.SetGoldText(_playerData.gold);

            string equippedName = _playerData.GetEquippedItemName(_equipmentType);
            var equippedIdentity = _equipmentManager.GetEquipmentByName(equippedName);
            int equippedLevel = _playerData.GetEquipmentTypeLevel(_equipmentType);

            _currentViewingIdentity = identity;
            _currentEquippedIdentity = equippedIdentity;
            _currentEquippedLevel = equippedLevel;

            if (!locked)
            {
                if (equipped)
                    UpdateViewEquipped(identity, equippedIdentity, equippedLevel);
                else
                    UpdateViewOwned(identity, equippedIdentity, equippedLevel);
            }
            else
            {
                UpdateViewLocked(identity, equippedIdentity, equippedLevel);
            }
        }

        // ── 미구매 장비 ──
        private void UpdateViewLocked(EquipmentIdentity identity, EquipmentIdentity equippedIdentity, int equippedLevel)
        {
            _view.SetActionButtonState(EquipmentActionButtonState.Purchase, identity.purchasePrice);
            _view.SetActionButtonInteractable(_playerData.gold >= identity.purchasePrice);

            // 미구매 장비는 Lv1 기준으로 표시 (캐릭터와 동일 정책)
            _currentViewingLevel = 1;

            var viewingStats = identity.GetStatsAtLevel(1);
            var equippedStats = equippedIdentity != null
                ? equippedIdentity.GetStatsAtLevel(equippedLevel)
                : default;
            var viewingEffects = identity.GetEffectDataAtLevel(1);
            var equippedEffects = equippedIdentity?.GetEffectDataAtLevel(equippedLevel);

            _view.SetCurrentStatsText("Level : 1\n" + LobbyStatFormatter.FormatStatsWithComparison(
                viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType,
                equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
            _view.SetCurrentStatsDetailButtonActive(true);
            _view.SetLevelGrowthStatText(LobbyStatFormatter.FormatGrowthStats(identity.levelStatGrowth, equippedIdentity?.levelStatGrowth ?? default, identity.effectGrowths, equippedIdentity?.effectGrowths));
            _view.SetGrowthDetailButtonActive(true);
        }

        // ── 현재 착용 중인 장비 ──
        private void UpdateViewEquipped(EquipmentIdentity identity, EquipmentIdentity equippedIdentity, int equippedLevel)
        {
            int level = equippedLevel; // 종류별 공유 레벨
            int cost = identity.GetLevelUpCost(level);
            bool isMaxLevel = level >= identity.maxLevel;

            _currentViewingLevel = level;

            _view.SetActionButtonState(EquipmentActionButtonState.LevelUp, isMaxLevel ? -1 : cost);
            if (!isMaxLevel)
                _view.SetActionButtonInteractable(_playerData.gold >= cost);

            var currentEffects = identity.GetEffectDataAtLevel(level);
            _view.SetCurrentStatsText($"Level : {level}\n" + LobbyStatFormatter.FormatStats(identity.GetStatsAtLevel(level), currentEffects));
            _view.SetCurrentStatsDetailButtonActive(true);

            if (!isMaxLevel)
            {
                _view.SetLevelGrowthStatText(LobbyStatFormatter.FormatGrowthStats(identity.levelStatGrowth, equippedIdentity?.levelStatGrowth ?? default, identity.effectGrowths, equippedIdentity?.effectGrowths));
                _view.SetGrowthDetailButtonActive(true);
            }
            else
            {
                _view.SetLevelGrowthStatText("<color=#FFD700>MAX LEVEL</color>");
                _view.SetGrowthDetailButtonActive(false);
            }
        }

        // ── 소유했지만 미착용 장비 ──
        private void UpdateViewOwned(EquipmentIdentity identity, EquipmentIdentity equippedIdentity, int equippedLevel)
        {
            // 종류별 공유 레벨이므로, 미착용 장비도 같은 레벨로 표시한다.
            int level = equippedLevel;

            _currentViewingLevel = level;

            _view.SetActionButtonState(EquipmentActionButtonState.Equip);

            var viewingStats = identity.GetStatsAtLevel(level);
            var equippedStats = equippedIdentity != null
                ? equippedIdentity.GetStatsAtLevel(equippedLevel)
                : default;
            var viewingEffects = identity.GetEffectDataAtLevel(level);
            var equippedEffects = equippedIdentity?.GetEffectDataAtLevel(equippedLevel);

            _view.SetCurrentStatsText($"Level : {level}\n" + LobbyStatFormatter.FormatStatsWithComparison(
                viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType,
                equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
            _view.SetCurrentStatsDetailButtonActive(true);

            if (level < identity.maxLevel)
            {
                _view.SetLevelGrowthStatText(LobbyStatFormatter.FormatGrowthStats(identity.levelStatGrowth, equippedIdentity?.levelStatGrowth ?? default, identity.effectGrowths, equippedIdentity?.effectGrowths));
                _view.SetGrowthDetailButtonActive(true);
            }
            else
            {
                _view.SetLevelGrowthStatText("<color=#FFD700>MAX LEVEL</color>");
                _view.SetGrowthDetailButtonActive(false);
            }
        }

        private void UpdateEquippedContents()
        {
            string equippedName = _playerData.GetEquippedItemName(_equipmentType);
            var equippedIdentity = _equipmentManager.GetEquipmentByName(equippedName);
            if (equippedIdentity == null) return;

            int level = _playerData.GetEquipmentTypeLevel(_equipmentType);

            _view.SetEquippedEquipmentIcon(equippedIdentity.equipmentIcon);
            _view.SetEquippedEquipmentName(equippedName);
            _view.SetEquippedLevelText($"Lv.{level}");
            var equippedEffectsForStats = equippedIdentity.GetEffectDataAtLevel(level);
            _view.SetEquippedStatsText(LobbyStatFormatter.FormatStats(equippedIdentity.GetStatsAtLevel(level), equippedEffectsForStats));
            _view.SetEquippedStatsDetailButtonActive(equippedEffectsForStats is { Count: > 0 });
        }

    }
}
