using System.Collections.Generic;
using Managers;
using Players;
using Stat;

namespace UI
{
    /// <summary>
    /// 캐릭터 탭 Presenter.
    /// 캐릭터 상태(착용 중/소유/미구매)에 따라 액션 버튼과 스탯 패널을 갱신한다.
    /// </summary>
    public class CharacterTabPresenter
    {
        private readonly UI_CharacterTabView _view;
        private readonly LobbyCharacterManager _lobbyCharacterManager;
        private readonly LobbyCharacterCamera _characterCamera;
        private readonly CharacterManager _characterManager;
        private readonly PlayerData _playerData;

        private CharacterIdentity _currentViewingIdentity;
        private CharacterIdentity _currentEquippedIdentity;
        private int _currentViewingLevel;
        private int _currentEquippedLevel;

        public CharacterTabPresenter(UI_CharacterTabView view,
            LobbyCharacterManager lobbyCharacterManager,
            LobbyCharacterCamera characterCamera)
        {
            _view = view;
            _lobbyCharacterManager = lobbyCharacterManager;
            _characterCamera = characterCamera;

            _characterManager = CharacterManager.Instance;
            _playerData = PlayerManager.Instance.PlayerData;

            _view.Init(OnLeft, OnRight, OnActionButton, OnGrowthDetail, OnCurrentStatsDetail, OnEquippedStatsDetail);
        }

        public void Activate()
        {
            var rt = _characterCamera.GetOrCreateRenderTexture();
            _view.SetRenderTexture(rt);
            UpdateView();
            UpdateEquippedContents();
        }

        public void Deactivate()
        {
            _view.CloseAllDetailPopups();
            _lobbyCharacterManager.SetLastValidState();
        }

        private void OnLeft()
        {
            _lobbyCharacterManager.ChangeCharacter(-1);
            UpdateView();
        }

        private void OnRight()
        {
            _lobbyCharacterManager.ChangeCharacter(1);
            UpdateView();
        }

        private void OnCurrentStatsDetail()
        {
            if (_currentViewingIdentity == null)
                return;
            string detail = LobbyStatFormatter.FormatCurrentStatsDetail(
                _currentViewingIdentity.GetEffectDataAtLevel(_currentViewingLevel),
                _currentEquippedIdentity?.GetEffectDataAtLevel(_currentEquippedLevel));
            _view.ShowCurrentStatsDetailPopup(detail);
        }

        private void OnGrowthDetail()
        {
            if (_currentViewingIdentity == null)
                return;
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
            string name = _lobbyCharacterManager.GetCurrentCharacterName();
            bool unlocked = _characterManager.IsCharacterUnlocked(name);
            bool equipped = _lobbyCharacterManager.IsCurrentCharacterEquipped();

            if (!unlocked)
            {
                // 구매
                if (_characterManager.TryPurchaseCharacter(name))
                {
                    UpdateView();
                    // 구매 후 자동 착용하지 않음
                }
            }
            else if (equipped)
            {
                if (_characterManager.TryLevelUpCharacter(name))
                {
                    UpdateView();
                    UpdateEquippedContents();
                }
            }
            else
            {
                _lobbyCharacterManager.SelectCurrent();
                PlayerManager.Instance.SetCurrentCharacter(
                    _lobbyCharacterManager.GetCurrentCharacterIdentity());
                UpdateView();
                UpdateEquippedContents();
            }
        }

        private void UpdateView()
        {
            string name = _lobbyCharacterManager.GetCurrentCharacterName();
            bool locked = _lobbyCharacterManager.IsCurrentCharacterLocked();
            bool equipped = _lobbyCharacterManager.IsCurrentCharacterEquipped();

            var identity = _lobbyCharacterManager.GetCurrentCharacterIdentity();
            if (identity == null) return;

            _view.SetCharacterName(name);
            _view.SetLockIconActive(locked);
            _view.SetGoldText(_playerData.gold);

            string equippedName = _playerData.currentCharacterName;
            var equippedIdentity = _characterManager.GetCharacterIdentityByName(equippedName);
            int equippedLevel = _playerData.GetCharacterLevel(equippedName);

            _currentViewingIdentity = identity;
            _currentEquippedIdentity = equippedIdentity;
            _currentEquippedLevel = equippedLevel;

            if (!locked)
            {
                if (equipped)
                    UpdateViewEquipped(name, identity, equippedIdentity, equippedLevel);
                else
                    UpdateViewOwned(name, identity, equippedIdentity, equippedLevel);
            }
            else
            {
                UpdateViewLocked(identity, equippedIdentity, equippedLevel);
            }
        }

        // ── 미구매 캐릭터 ──
        private void UpdateViewLocked(CharacterIdentity identity, CharacterIdentity equippedIdentity, int equippedLevel)
        {
            _view.SetActionButtonState(CharacterActionButtonState.Purchase, identity.purchasePrice);
            _view.SetActionButtonInteractable(_playerData.gold >= identity.purchasePrice);

            _currentViewingLevel = 1;

            var viewingStats = identity.GetStatsAtLevel(1);
            var equippedStats = equippedIdentity?.GetStatsAtLevel(equippedLevel) ?? default;
            var viewingEffects = identity.GetEffectDataAtLevel(1);
            var equippedEffects = equippedIdentity?.GetEffectDataAtLevel(equippedLevel);

            _view.SetCurrentStatsText("Level : 1\n" + LobbyStatFormatter.FormatStatsWithComparison(viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType, equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
            _view.SetCurrentStatsDetailButtonActive(true);
            _view.SetLevelGrowthStatText(LobbyStatFormatter.FormatGrowthStats(identity.levelStatGrowth, equippedIdentity?.levelStatGrowth ?? default, identity.effectGrowths, equippedIdentity?.effectGrowths));
            _view.SetGrowthDetailButtonActive(true);
        }

        // ── 현재 착용 중인 캐릭터 ──
        private void UpdateViewEquipped(string name, CharacterIdentity identity, CharacterIdentity equippedIdentity, int equippedLevel)
        {
            int level = _playerData.GetCharacterLevel(name);
            int cost = identity.GetLevelUpCost(level);
            bool isMaxLevel = level >= identity.maxLevel;

            _currentViewingLevel = level;

            _view.SetActionButtonState(CharacterActionButtonState.LevelUp, isMaxLevel ? -1 : cost);
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

        // ── 소유했지만 미착용 캐릭터 ──
        private void UpdateViewOwned(string name, CharacterIdentity identity, CharacterIdentity equippedIdentity, int equippedLevel)
        {
            int level = _playerData.GetCharacterLevel(name);

            _currentViewingLevel = level;

            _view.SetActionButtonState(CharacterActionButtonState.Equip);

            var viewingStats = identity.GetStatsAtLevel(level);
            var equippedStats = equippedIdentity?.GetStatsAtLevel(equippedLevel) ?? default;
            var viewingEffects = identity.GetEffectDataAtLevel(level);
            var equippedEffects = equippedIdentity?.GetEffectDataAtLevel(equippedLevel);

            _view.SetCurrentStatsText($"Level : {level}\n" + LobbyStatFormatter.FormatStatsWithComparison(viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType, equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
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
            string equippedName = _playerData.currentCharacterName;
            var equippedIdentity = _characterManager.GetCharacterIdentityByName(equippedName);
            if (equippedIdentity == null) return;

            int level = _playerData.GetCharacterLevel(equippedName);

            _view.SetEquippedCharacterIcon(equippedIdentity.characterIcon);
            _view.SetEquippedCharacterName(equippedName);
            _view.SetEquippedLevelText($"Lv.{level}");
            var equippedEffectsForStats = equippedIdentity.GetEffectDataAtLevel(level);
            _view.SetEquippedStatsText(LobbyStatFormatter.FormatStats(equippedIdentity.GetStatsAtLevel(level), equippedEffectsForStats));
            _view.SetEquippedStatsDetailButtonActive(equippedEffectsForStats is { Count: > 0 });
        }

    }
}
