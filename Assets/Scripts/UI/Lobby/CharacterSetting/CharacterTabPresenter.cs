using System.Collections.Generic;
using System.Linq;
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
            string detail = FormatCurrentStatsDetail(
                _currentViewingIdentity, _currentViewingLevel,
                _currentEquippedIdentity, _currentEquippedLevel);
            _view.ShowCurrentStatsDetailPopup(detail);
        }

        private void OnGrowthDetail()
        {
            if (_currentViewingIdentity == null)
                return;
            string detail = FormatGrowthStatsDetail(_currentViewingIdentity, _currentEquippedIdentity);
            _view.ShowGrowthDetailPopup(detail);
        }

        private void OnEquippedStatsDetail()
        {
            if (_currentEquippedIdentity == null) return;
            var effectMap = _currentEquippedIdentity.GetEffectDataAtLevel(_currentEquippedLevel);
            string detail = FormatEquippedStatsDetail(effectMap);
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

            _view.SetCurrentStatsText("Level : 1\n" + FormatStatsWithComparison(viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType, equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
            _view.SetCurrentStatsDetailButtonActive(true);
            _view.SetLevelGrowthStatText(FormatGrowthStats(identity, equippedIdentity));
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
            _view.SetCurrentStatsText($"Level : {level}\n" + FormatStats(identity.GetStatsAtLevel(level), currentEffects));
            _view.SetCurrentStatsDetailButtonActive(true);

            if (!isMaxLevel)
            {
                _view.SetLevelGrowthStatText(FormatGrowthStats(identity, equippedIdentity));
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

            _view.SetCurrentStatsText($"Level : {level}\n" + FormatStatsWithComparison(viewingStats, equippedStats, viewingEffects, equippedEffects,
                identity.baseStat.attackEffectType, equippedIdentity?.baseStat.attackEffectType ?? EffectType.Normal));
            _view.SetCurrentStatsDetailButtonActive(true);

            if (level < identity.maxLevel)
            {
                _view.SetLevelGrowthStatText(FormatGrowthStats(identity, equippedIdentity));
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
            _view.SetEquippedStatsText(FormatStats(equippedIdentity.GetStatsAtLevel(level), equippedEffectsForStats));
            _view.SetEquippedStatsDetailButtonActive(equippedEffectsForStats is { Count: > 0 });
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
                var effectNames = new System.Collections.Generic.List<string>();
                foreach (var kvp in effectMap)
                {
                    if (kvp.Key == EffectType.Normal) continue;
                    if (!Utils.HasEffectType(stat.attackEffectType, kvp.Key)) continue;
                    effectNames.Add($"<color={GetEffectColor(kvp.Key)}>{GetEffectLabel(kvp.Key)}</color>");
                }
                if (effectNames.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append($"Magic Effect: {string.Join(", ", effectNames)}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// currentStatsText용: 자신의 이펙트를 ▲(초록)으로 표시.
        /// </summary>
        private static string FormatStatsWithEffectArrows(CharacterBaseStatData stat, Dictionary<EffectType, EffectData> effectMap)
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
                    string label = GetEffectLabel(kvp.Key);
                    string color = GetEffectColor(kvp.Key);
                    sb.AppendLine();
                    sb.Append($"<color={color}>{label}</color> <color=#22C55E>▲</color>");
                }
            }

            return sb.ToString();
        }

        private static string FormatEquippedStatsDetail(Dictionary<EffectType, EffectData> effectMap)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700>Effect Detail</color>");

            var allTypes = new[] { EffectType.Fire, EffectType.Poison, EffectType.Ice, EffectType.Lightning, EffectType.Magma, EffectType.Dark };
            foreach (var effectType in allTypes)
            {
                EffectData dRaw = null;
                effectMap?.TryGetValue(effectType, out dRaw);
                var d = dRaw ?? new EffectData();
                string label = GetEffectLabel(effectType);
                string color = GetEffectColor(effectType);
                sb.AppendLine($"<color={color}>{label}</color>");
                sb.AppendLine($"  dur: {d.duration:F1}s");
                if (IsDotEffect(effectType))
                    sb.AppendLine($"  dot: {d.dotDamage:F1} / {d.tickInterval:F1}s");
                else
                    sb.AppendLine($"  val: {d.value:F1}%");
            }

            TrimTrailingNewline(sb);
            return sb.ToString();
        }

        /// <summary>
        /// 보고 있는 캐릭터 스탯과 현재 장착 캐릭터 스탯을 비교하여 ▲▼ 표시로 포맷한다.
        /// 이펙트 추가/제거도 한 줄 요약으로 포함.
        /// </summary>
        private static string FormatStatsWithComparison(
            CharacterBaseStatData viewing, CharacterBaseStatData equipped,
            Dictionary<EffectType, EffectData> viewingEffects, Dictionary<EffectType, EffectData> equippedEffects,
            EffectType viewingEffectType, EffectType equippedEffectType)
        {
            var sb = new StringBuilder();
            sb.AppendLine(FormatCompareInt("HP", viewing.maxHP, equipped.maxHP));
            sb.AppendLine(FormatCompareInt("ATK", viewing.attackPower, equipped.attackPower));
            sb.AppendLine(FormatCompareFloat("SPD", viewing.moveSpeed, equipped.moveSpeed, "F1"));
            sb.AppendLine(FormatCompareInt("ARM", viewing.armor, equipped.armor));
            sb.AppendLine(FormatCompareInt("MR", viewing.magicResistance, equipped.magicResistance));
            sb.Append(FormatCompareFloat("AS", viewing.attackSpeed, equipped.attackSpeed, "F2"));

            AppendEffectDataSummary(sb, viewingEffects, equippedEffects, viewingEffectType, equippedEffectType);

            return sb.ToString();
        }

        /// <summary>
        /// 이펙트 추가/제거/변화를 한 줄 요약으로 추가. 변화 없으면 미표시.
        /// 예) 새로 생긴 이펙트 → 녹색▲, 없어진 이펙트 → 빨간▼
        /// </summary>
        private static void AppendEffectDataSummary(
            StringBuilder sb,
            Dictionary<EffectType, EffectData> viewingEffects,
            Dictionary<EffectType, EffectData> equippedEffects,
            EffectType viewingEffectType,
            EffectType equippedEffectType)
        {
            var allTypes = new HashSet<EffectType>();
            if (viewingEffects != null)
                foreach (var k in viewingEffects.Keys) allTypes.Add(k);
            if (equippedEffects != null)
                foreach (var k in equippedEffects.Keys) allTypes.Add(k);

            // viewing 캐릭터 속성 헤더용, ▲▼ 라인 분리 수집
            var headerNames = new System.Collections.Generic.List<string>();
            var buffLines   = new System.Collections.Generic.List<string>();
            var nerfLines   = new System.Collections.Generic.List<string>();

            foreach (var effectType in allTypes)
            {
                if (effectType == EffectType.Normal) continue;

                bool viewingHas = viewingEffects != null && viewingEffects.TryGetValue(effectType, out var vd)
                                  && Utils.HasEffectType(viewingEffectType, effectType);
                bool equippedHas = equippedEffects != null && equippedEffects.TryGetValue(effectType, out var ed)
                                   && Utils.HasEffectType(equippedEffectType, effectType);

                string label = GetEffectLabel(effectType);
                string color = GetEffectColor(effectType);
                string coloredLabel = $"<color={color}>{label}</color>";

                if (viewingHas)
                    headerNames.Add(coloredLabel);

                if (viewingHas && !equippedHas)
                    buffLines.Add($"{coloredLabel} <color=#22C55E>▲</color>");
                else if (!viewingHas && equippedHas)
                    nerfLines.Add($"{coloredLabel} <color=#EF4444>▼</color>");
            }

            // Magic Effect 헤더 (viewing 속성이 하나라도 있으면)
            if (headerNames.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"Magic Effect: {string.Join(", ", headerNames)}");
            }
            // 변화 있는 속성만 ▲▼ 한 줄씩
            foreach (var line in buffLines)
            {
                sb.AppendLine();
                sb.Append(line);
            }
            foreach (var line in nerfLines)
            {
                sb.AppendLine();
                sb.Append(line);
            }
        }

        /// <summary>
        /// effectData가 있는지 확인. (상세 버튼 활성화 여부 판단)
        /// </summary>
        private static bool HasEffects(
            Dictionary<EffectType, EffectData> viewingEffects,
            Dictionary<EffectType, EffectData> equippedEffects)
        {
            bool v = viewingEffects is { Count: > 0 };
            bool e = equippedEffects is { Count: > 0 };
            return v || e;
        }

        /// <summary>
        /// 현재 레벨 effectData 상세를 포맷한다. (현재 스탯 상세 팝업용)
        /// </summary>
        private static string FormatCurrentStatsDetail(
            CharacterIdentity viewingIdentity, int viewingLevel,
            CharacterIdentity equippedIdentity, int equippedLevel)
        {
            var viewingEffects = viewingIdentity.GetEffectDataAtLevel(viewingLevel);
            var equippedEffects = equippedIdentity?.GetEffectDataAtLevel(equippedLevel);

            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700>Effect Detail</color>");

            var allTypes = new[] { EffectType.Fire, EffectType.Poison, EffectType.Ice, EffectType.Lightning, EffectType.Magma, EffectType.Dark };
            foreach (var effectType in allTypes)
            {
                EffectData vdRaw = null;
                EffectData edRaw = null;

                viewingEffects?.TryGetValue(effectType, out vdRaw);
                equippedEffects?.TryGetValue(effectType, out edRaw);

                var vd = vdRaw ?? new EffectData();
                var ed = edRaw ?? new EffectData();

                string label = GetEffectLabel(effectType);
                string color = GetEffectColor(effectType);
                sb.AppendLine($"<color={color}>{label}</color>");
                sb.AppendLine(FormatCompareFloat("  dur", vd.duration, ed.duration, "F1", "", "s"));
                if (IsDotEffect(effectType))
                {
                    sb.AppendLine(FormatCompareFloat("  dot", vd.dotDamage, ed.dotDamage, "F1"));
                    sb.AppendLine(FormatCompareFloat("  tick", vd.tickInterval, ed.tickInterval, "F1", "", "s"));
                }
                else
                {
                    sb.AppendLine(FormatCompareFloat("  val", vd.value, ed.value, "F1", "", "%"));
                }
            }

            TrimTrailingNewline(sb);
            return sb.ToString();
        }

        private static string FormatCompareInt(string label, int value, int equippedValue, string prefix = "")
        {
            int diff = value - equippedValue;
            if (diff > 0)
                return $"{label}: {prefix}{value}  <color=#22C55E>▲{diff}</color>";
            if (diff < 0)
                return $"{label}: {prefix}{value}  <color=#EF4444>▼{-diff}</color>";
            return $"{label}: {prefix}{value}";
        }

        private static string FormatCompareFloat(string label, float value, float equippedValue, string fmt, string prefix = "", string suffix = "")
        {
            float diff = value - equippedValue;
            string valStr = value.ToString(fmt);
            string diffStr = UnityEngine.Mathf.Abs(diff).ToString(fmt);
            if (diff > 0.001f)
                return $"{label}: {prefix}{valStr}{suffix}  <color=#22C55E>▲{diffStr}{suffix}</color>";
            if (diff < -0.001f)
                return $"{label}: {prefix}{valStr}{suffix}  <color=#EF4444>▼{diffStr}{suffix}</color>";
            return $"{label}: {prefix}{valStr}{suffix}";
        }

        /// <summary>
        /// 레벨당 성장 스탯 요약을 포맷한다.
        /// 기본 스탯은 ▲▼ 비교, 이펙트는 변화 여부만 한 줄로 표시.
        /// </summary>
        private static string FormatGrowthStats(CharacterIdentity identity, CharacterIdentity equippedIdentity)
        {
            var g = identity.levelStatGrowth;
            var eg = equippedIdentity?.levelStatGrowth ?? default;

            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700>Per Level</color>");
            if (g.maxHP != 0 || eg.maxHP != 0)
                sb.AppendLine(FormatCompareInt("HP", g.maxHP, eg.maxHP, "+"));
            if (g.attackPower != 0 || eg.attackPower != 0)
                sb.AppendLine(FormatCompareInt("ATK", g.attackPower, eg.attackPower, "+"));
            if (g.moveSpeed != 0f || eg.moveSpeed != 0f)
                sb.AppendLine(FormatCompareFloat("SPD", g.moveSpeed, eg.moveSpeed, "F1", "+"));
            if (g.armor != 0 || eg.armor != 0)
                sb.AppendLine(FormatCompareInt("ARM", g.armor, eg.armor, "+"));
            if (g.magicResistance != 0 || eg.magicResistance != 0)
                sb.AppendLine(FormatCompareInt("MR", g.magicResistance, eg.magicResistance, "+"));
            if (g.attackSpeed != 0f || eg.attackSpeed != 0f)
                sb.AppendLine(FormatCompareFloat("AS", g.attackSpeed, eg.attackSpeed, "F2", "+"));
            if (g.projectileSpeed != 0f || eg.projectileSpeed != 0f)
                sb.AppendLine(FormatCompareFloat("PS", g.projectileSpeed, eg.projectileSpeed, "F1", "+"));

            // ── 이펙트 성장: 변화가 있는 것만 한 줄 요약 ──
            AppendEffectGrowthSummary(sb, identity, equippedIdentity);

            TrimTrailingNewline(sb);
            return sb.ToString();
        }

        /// <summary>
        /// 이펙트 성장의 변화 여부를 한 줄로 요약. 변화 없으면 표시하지 않는다.
        /// ex) "Fire ▲", "Ice ▼"
        /// </summary>
        private static void AppendEffectGrowthSummary(StringBuilder sb,
            CharacterIdentity identity, CharacterIdentity equippedIdentity)
        {
            var viewingEffects = identity.effectGrowths;
            var equippedEffects = equippedIdentity?.effectGrowths;

            var allEffectTypes = new HashSet<EffectType>();
            if (viewingEffects != null)
                foreach (var e in viewingEffects) allEffectTypes.Add(e.effectType);
            if (equippedEffects != null)
                foreach (var e in equippedEffects) allEffectTypes.Add(e.effectType);

            // viewing 캐릭터 성장 속성 헤더용, ▲▼ 라인 분리 수집
            var growthHeaderNames = new System.Collections.Generic.List<string>();
            var growthBuffLines   = new System.Collections.Generic.List<string>();
            var growthNerfLines   = new System.Collections.Generic.List<string>();

            foreach (var effectType in allEffectTypes)
            {
                if (effectType == EffectType.Normal) continue;

                var vg = viewingEffects?.FirstOrDefault(e => e.effectType == effectType) ?? default;
                var eqg = equippedEffects?.FirstOrDefault(e => e.effectType == effectType) ?? default;

                bool viewingHasGrowth = viewingEffects != null &&
                    viewingEffects.Any(e => e.effectType == effectType);

                int cmp = CompareEffectGrowth(vg, eqg);
                string label = GetEffectLabel(effectType);
                string color = GetEffectColor(effectType);
                string coloredLabel = $"<color={color}>{label}</color>";

                if (viewingHasGrowth)
                    growthHeaderNames.Add(coloredLabel);

                if (cmp > 0)
                    growthBuffLines.Add($"{coloredLabel} <color=#22C55E>▲</color>");
                else if (cmp < 0)
                    growthNerfLines.Add($"{coloredLabel} <color=#EF4444>▼</color>");
            }

            // Magic Effect 헤더 (viewing 성장 속성이 하나라도 있으면)
            if (growthHeaderNames.Count > 0)
                sb.AppendLine($"Magic Effect: {string.Join(", ", growthHeaderNames)}");
            // 변화 있는 속성만 ▲▼ 한 줄씩
            foreach (var line in growthBuffLines)
                sb.AppendLine(line);
            foreach (var line in growthNerfLines)
                sb.AppendLine(line);
        }

        /// <summary>
        /// 두 이펙트 성장을 비교. 양수=viewing이 우세, 음수=equipped가 우세, 0=동일.
        /// </summary>
        private static int CompareEffectGrowth(CharacterEffectGrowth a, CharacterEffectGrowth b)
        {
            float totalA = a.durationGrowth + a.valueGrowth + a.dotDamageGrowth;
            float totalB = b.durationGrowth + b.valueGrowth + b.dotDamageGrowth;
            float diff = totalA - totalB;
            if (diff > 0.001f) return 1;
            if (diff < -0.001f) return -1;
            return 0;
        }

        /// <summary>
        /// 성장 스탯 상세 정보를 포맷한다. (디테일 팝업용)
        /// </summary>
        public static string FormatGrowthStatsDetail(CharacterIdentity identity, CharacterIdentity equippedIdentity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700>Effect Growth Detail</color>");

            var viewingEffects = identity.effectGrowths;
            var equippedEffects = equippedIdentity?.effectGrowths;

            var allTypes = new[] { EffectType.Fire, EffectType.Poison, EffectType.Ice, EffectType.Lightning, EffectType.Magma, EffectType.Dark };
            foreach (var effectType in allTypes)
            {
                var vg = viewingEffects?.FirstOrDefault(e => e.effectType == effectType) ?? default;
                var eqg = equippedEffects?.FirstOrDefault(e => e.effectType == effectType) ?? default;

                string label = GetEffectLabel(effectType);
                string color = GetEffectColor(effectType);
                sb.AppendLine($"<color={color}>{label}</color>");
                sb.AppendLine(FormatCompareFloat("  dur", vg.durationGrowth, eqg.durationGrowth, "F2", "+", "s"));
                if (IsDotEffect(effectType))
                {
                    sb.AppendLine(FormatCompareFloat("  dot", vg.dotDamageGrowth, eqg.dotDamageGrowth, "F1", "+"));
                    sb.AppendLine(FormatCompareFloat("  tick", vg.tickIntervalGrowth, eqg.tickIntervalGrowth, "F2", "+", "s"));
                }
                else
                {
                    sb.AppendLine(FormatCompareFloat("  val", vg.valueGrowth, eqg.valueGrowth, "F2", "+", "%"));
                }
            }

            TrimTrailingNewline(sb);
            return sb.ToString();
        }

        private static void TrimTrailingNewline(StringBuilder sb)
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '\n')
                sb.Length -= 1;
            if (sb.Length > 0 && sb[sb.Length - 1] == '\r')
                sb.Length -= 1;
        }

        /// <summary>
        /// Fire/Poison 처럼 dotDamage 기반인지, Ice 등 value(%) 기반인지 분류.
        /// </summary>
        private static bool IsDotEffect(EffectType type) => type switch
        {
            EffectType.Fire => true,
            EffectType.Poison => true,
            _ => false,
        };

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
