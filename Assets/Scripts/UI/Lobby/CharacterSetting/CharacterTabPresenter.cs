using System.Collections.Generic;
using System.Text;
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
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(1), identity.GetEffectDataAtLevel(1)));
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
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(level), identity.GetEffectDataAtLevel(level)));

                if (!isMaxLevel)
                    _view.SetNextLevelStatsText($"<color=#FFD700>Next Lv.{level + 1}</color>\n{FormatStats(identity.GetStatsAtLevel(level + 1), identity.GetEffectDataAtLevel(level + 1))}");
                else
                    _view.SetNextLevelStatsText("<color=#FFD700>MAX LEVEL</color>");
            }
            else
            {
                // ── 소유했지만 미착용 캐릭터 ──
                int level = _playerData.GetCharacterLevel(name);

                _view.SetActionButtonState(CharacterActionButtonState.Equip);

                _view.SetLevelText($"Lv.{level}");
                _view.SetCurrentStatsText(FormatStats(identity.GetStatsAtLevel(level), identity.GetEffectDataAtLevel(level)));

                if (level < identity.maxLevel)
                    _view.SetNextLevelStatsText($"<color=#FFD700>Next Lv.{level + 1}</color>\n{FormatStats(identity.GetStatsAtLevel(level + 1), identity.GetEffectDataAtLevel(level + 1))}");
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
            _view.SetEquippedStatsText(FormatStats(equippedIdentity.GetStatsAtLevel(level), equippedIdentity.GetEffectDataAtLevel(level)));
        }

        private static string FormatStats(CharacterBaseStatData stat, Dictionary<EffectType, EffectData> effectMap = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"HP: {stat.maxHP}");
            sb.AppendLine($"ATK: {stat.attackPower}");
            sb.AppendLine($"SPD: {stat.moveSpeed:F1}");
            sb.AppendLine($"ARM: {stat.armor}");
            sb.AppendLine($"MR: {stat.magicResistance}");
            sb.Append($"AS: {stat.attackSpeed:F2}");

            if (stat.attackEffectType != EffectType.Normal && effectMap is { Count: > 0 })
            {
                foreach (var kvp in effectMap)
                {
                    if (kvp.Key == EffectType.Normal) continue;
                    if (!Utils.HasEffectType(stat.attackEffectType, kvp.Key)) continue;

                    var d = kvp.Value;
                    string label = GetEffectLabel(kvp.Key);
                    string color = GetEffectColor(kvp.Key);

                    sb.AppendLine();
                    sb.Append($"<color={color}>{label}</color> ");
                    sb.Append($"dur:{d.duration:F1}s val:{d.value:F2}");
                    if (d.dotDamage > 0)
                        sb.Append($" dot:{d.dotDamage:F1}/{d.tickInterval:F1}s");
                }
            }

            return sb.ToString();
        }

        private static string GetEffectLabel(EffectType type) => type switch
        {
            EffectType.Fire => "Fire",
            EffectType.Poison => "Poison",
            EffectType.Ice => "Ice",
            EffectType.Lightning => "Lightning",
            EffectType.Magma => "Magma",
            EffectType.Dark => "Dark",
            _ => type.ToString(),
        };

        private static string GetEffectColor(EffectType type) => type switch
        {
            EffectType.Fire => "#FF6B35",
            EffectType.Poison => "#A855F7",
            EffectType.Ice => "#38BDF8",
            EffectType.Lightning => "#FACC15",
            EffectType.Magma => "#F97316",
            EffectType.Dark => "#6B7280",
            _ => "#FFFFFF",
        };
    }
}
