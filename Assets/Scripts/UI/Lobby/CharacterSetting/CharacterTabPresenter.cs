using Managers;
using Players;

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

        public CharacterTabPresenter(UI_CharacterTabView view,
            LobbyCharacterManager lobbyCharacterManager,
            LobbyCharacterCamera characterCamera)
        {
            _view = view;
            _lobbyCharacterManager = lobbyCharacterManager;
            _characterCamera = characterCamera;

            _characterManager = CharacterManager.Instance;
            _playerData = PlayerManager.Instance.PlayerData;

            _view.Init(OnLeft, OnRight, OnActionButton);
        }

        public void Activate()
        {
            var rt = _characterCamera.GetOrCreateRenderTexture();
            _view.SetRenderTexture(rt);
            UpdateView();
            UpdateEquippedContents();
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

            bool unlocked = !locked;

            if (!unlocked)
            {
                _view.SetActionButtonState(CharacterActionButtonState.Purchase, identity.purchasePrice);
                _view.SetActionButtonInteractable(_playerData.gold >= identity.purchasePrice);

                _view.SetLevelText("Lv.1");
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(1)));
                _view.SetNextLevelStatsText("");
            }
            else if (equipped)
            {
                int level = _playerData.GetCharacterLevel(name);
                int cost = identity.GetLevelUpCost(level);
                bool isMaxLevel = level >= identity.maxLevel;

                _view.SetActionButtonState(CharacterActionButtonState.LevelUp, isMaxLevel ? -1 : cost);
                if (!isMaxLevel)
                    _view.SetActionButtonInteractable(_playerData.gold >= cost);

                _view.SetLevelText($"Lv.{level}");
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(level)));

                if (!isMaxLevel)
                    _view.SetNextLevelStatsText($"<color=#FFD700>Next Lv.{level + 1}</color>\n{FormatStats(identity.GetStatsAtLevel(level + 1))}");
                else
                    _view.SetNextLevelStatsText("<color=#FFD700>MAX LEVEL</color>");
            }
            else
            {
                // ── 소유했지만 미착용 캐릭터 ──
                int level = _playerData.GetCharacterLevel(name);

                _view.SetActionButtonState(CharacterActionButtonState.Equip);

                _view.SetLevelText($"Lv.{level}");
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(level)));

                if (level < identity.maxLevel)
                    _view.SetNextLevelStatsText($"<color=#FFD700>Next Lv.{level + 1}</color>\n{FormatStats(identity.GetStatsAtLevel(level + 1))}");
                else
                    _view.SetNextLevelStatsText("<color=#FFD700>MAX LEVEL</color>");
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
            _view.SetEquippedStatsText(FormatStats(equippedIdentity.GetStatsAtLevel(level)));
        }

        private static string FormatStats(CharacterBaseStatData stat)
        {
            return $"❤️ HP: {stat.maxHP}\n" +
                   $"⚔️ ATK: {stat.attackPower}\n" +
                   $"💨 SPD: {stat.moveSpeed:F1}\n" +
                   $"🛡️ ARM: {stat.armor}\n" +
                   $"✨ MR: {stat.magicResistance}\n" +
                   $"⚡ AS: {stat.attackSpeed:F2}";
        }
    }
}
