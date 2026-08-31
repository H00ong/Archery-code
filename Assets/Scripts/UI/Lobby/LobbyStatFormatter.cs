using System.Collections.Generic;
using System.Text;
using Players;
using Stat;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 로비 UI(CharacterTabPresenter, EquipmentTabPresenter)에서 공용으로 사용하는
    /// 스탯 텍스트 포맷 유틸리티 클래스.
    /// </summary>
    public static class LobbyStatFormatter
    {
        /// <summary>
        /// 스탯 텍스트를 기본 스탯 블록과 Magic Effect 블록으로 분리해 담는 구조체.
        /// Magic Effect를 별도 TMP 필드에 배치하면 상세 버튼을 바로 옆에 붙일 수 있고,
        /// 이펙트가 많아져도 기본 스탯 영역을 침범(overflow)하지 않는다.
        /// </summary>
        public readonly struct StatText
        {
            /// <summary> HP/ATK/SPD/ARM/MR/AS 등 기본 스탯 블록. (Magic Effect 제외) </summary>
            public readonly string Stats;
            /// <summary> Magic Effect 헤더 + ▲▼ 줄. 이펙트가 없으면 빈 문자열. </summary>
            public readonly string MagicEffect;

            public StatText(string stats, string magicEffect)
            {
                Stats = stats;
                MagicEffect = magicEffect ?? string.Empty;
            }

            public bool HasMagicEffect => !string.IsNullOrEmpty(MagicEffect);

            /// <summary> 기본 스탯 + Magic Effect를 하나로 합친 텍스트. Magic Effect가 없으면 Stats만 반환한다. </summary>
            public string Combined => HasMagicEffect
                ? (string.IsNullOrEmpty(Stats) ? MagicEffect : Stats + "\n" + MagicEffect)
                : Stats;
        }

        // ─────────────────────────────────────────────────────
        // 스탯 포맷
        // ─────────────────────────────────────────────────────

        public static StatText FormatStats(BaseStatData stat, Dictionary<EffectType, EffectData> effectMap = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"HP: {stat.maxHP}");
            sb.AppendLine($"ATK: {stat.attackPower}");
            sb.AppendLine($"SPD: {stat.moveSpeed:F1}");
            sb.AppendLine($"ARM: {stat.armor}");
            sb.AppendLine($"MR: {stat.magicResistance}");
            sb.Append($"AS: {stat.attackSpeed:F2}");

            return new StatText(sb.ToString(), BuildSimpleEffectSummary(stat, effectMap));
        }

        /// <summary>
        /// 보고 있는 스탯과 현재 장착 스탯을 비교하여 ▲▼ 표시로 포맷한다.
        /// </summary>
        public static StatText FormatStatsWithComparison(
            BaseStatData viewing, BaseStatData equipped,
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

            string magicEffect = BuildEffectDataSummary(viewingEffects, equippedEffects, viewingEffectType, equippedEffectType);
            return new StatText(sb.ToString(), magicEffect);
        }

        // ─────────────────────────────────────────────────────
        // 이펙트 상세 포맷
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// 장착 중인 아이템의 이펙트 상세 텍스트. (장착 패널 상세 팝업용)
        /// </summary>
        public static string FormatEquippedStatsDetail(Dictionary<EffectType, EffectData> effectMap)
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
        /// 현재 레벨 이펙트 상세 비교. (현재 스탯 상세 팝업용)
        /// </summary>
        public static string FormatCurrentStatsDetail(
            Dictionary<EffectType, EffectData> viewingEffects,
            Dictionary<EffectType, EffectData> equippedEffects)
        {
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

        // ─────────────────────────────────────────────────────
        // 성장 스탯 포맷
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// 레벨당 성장 스탯 요약. 기본 스탯은 ▲▼ 비교, 이펙트는 변화 여부만 한 줄 표시.
        /// </summary>
        public static StatText FormatGrowthStats(
            LevelStatGrowth growth, LevelStatGrowth equippedGrowth,
            EffectGrowth[] viewingEffectGrowths, EffectGrowth[] equippedEffectGrowths)
        {
            var g = growth;
            var eg = equippedGrowth;

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

            TrimTrailingNewline(sb);

            string magicEffect = BuildEffectGrowthSummary(viewingEffectGrowths, equippedEffectGrowths);
            return new StatText(sb.ToString(), magicEffect);
        }

        /// <summary>
        /// 성장 스탯 이펙트 상세. (성장 상세 팝업용)
        /// </summary>
        public static string FormatGrowthStatsDetail(
            EffectGrowth[] viewingEffects, EffectGrowth[] equippedEffects)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700>Effect Growth Detail</color>");

            var allTypes = new[] { EffectType.Fire, EffectType.Poison, EffectType.Ice, EffectType.Lightning, EffectType.Magma, EffectType.Dark };
            foreach (var effectType in allTypes)
            {
                var vg = FindEffectGrowth(viewingEffects, effectType);
                var eqg = FindEffectGrowth(equippedEffects, effectType);

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

        // ─────────────────────────────────────────────────────
        // 비교 포맷 헬퍼
        // ─────────────────────────────────────────────────────

        public static string FormatCompareInt(string label, int value, int equippedValue, string prefix = "")
        {
            int diff = value - equippedValue;
            if (diff > 0)
                return $"{label}: {prefix}{value}  <color=#22C55E>▲{diff}</color>";
            if (diff < 0)
                return $"{label}: {prefix}{value}  <color=#EF4444>▼{-diff}</color>";
            return $"{label}: {prefix}{value}";
        }

        public static string FormatCompareFloat(string label, float value, float equippedValue, string fmt, string prefix = "", string suffix = "")
        {
            float diff = value - equippedValue;
            string valStr = value.ToString(fmt);
            string diffStr = Mathf.Abs(diff).ToString(fmt);
            if (diff > 0.001f)
                return $"{label}: {prefix}{valStr}{suffix}  <color=#22C55E>▲{diffStr}{suffix}</color>";
            if (diff < -0.001f)
                return $"{label}: {prefix}{valStr}{suffix}  <color=#EF4444>▼{diffStr}{suffix}</color>";
            return $"{label}: {prefix}{valStr}{suffix}";
        }

        // ─────────────────────────────────────────────────────
        // 이펙트 유틸
        // ─────────────────────────────────────────────────────

        public static bool IsDotEffect(EffectType type) => type switch
        {
            EffectType.Fire => true,
            EffectType.Poison => true,
            _ => false,
        };

        public static string GetEffectLabel(EffectType type) => type switch
        {
            EffectType.Fire => "Fire",
            EffectType.Poison => "Poison",
            EffectType.Ice => "Ice",
            EffectType.Lightning => "Lightning",
            EffectType.Magma => "Magma",
            EffectType.Dark => "Dark",
            _ => type.ToString(),
        };

        public static string GetEffectColor(EffectType type) => type switch
        {
            EffectType.Fire => "#FF6B35",
            EffectType.Poison => "#A855F7",
            EffectType.Ice => "#38BDF8",
            EffectType.Lightning => "#FACC15",
            EffectType.Magma => "#F97316",
            EffectType.Dark => "#6B7280",
            _ => "#FFFFFF",
        };

        public static void TrimTrailingNewline(StringBuilder sb)
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '\n')
                sb.Length -= 1;
            if (sb.Length > 0 && sb[sb.Length - 1] == '\r')
                sb.Length -= 1;
        }

        // ─────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────
        // 내부 헬퍼 — Magic Effect 블록을 별도 문자열로 생성
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// 비교 없이 자신의 Magic Effect 이름만 한 줄로 나열한다. (FormatStats 전용) 없으면 빈 문자열.
        /// </summary>
        private static string BuildSimpleEffectSummary(BaseStatData stat, Dictionary<EffectType, EffectData> effectMap)
        {
            if (stat.attackEffectType == EffectType.Normal || effectMap is not { Count: > 0 })
                return string.Empty;

            var effectNames = new List<string>();
            foreach (var kvp in effectMap)
            {
                if (kvp.Key == EffectType.Normal) continue;
                if (!Utils.HasEffectType(stat.attackEffectType, kvp.Key)) continue;
                effectNames.Add($"<color={GetEffectColor(kvp.Key)}>{GetEffectLabel(kvp.Key)}</color>");
            }
            if (effectNames.Count == 0)
                return string.Empty;

            return "Magic Effect\n" + string.Join("\n", effectNames);
        }

        /// <summary> 비교 대상 대비 이펙트 한 줄의 변화 상태. </summary>
        private enum EffectChangeState { None, Buff, Nerf, Mixed }

        /// <summary> 이펙트 이름 + 변화 상태에 따른 화살표(▲▼)/네모(■, 부분 증감 혼재)를 한 줄로 합친다. </summary>
        private static string FormatEffectLine(EffectType effectType, EffectChangeState state)
        {
            string coloredLabel = $"<color={GetEffectColor(effectType)}>{GetEffectLabel(effectType)}</color>";
            return state switch
            {
                EffectChangeState.Buff => $"{coloredLabel} <color=#22C55E>▲</color>",
                EffectChangeState.Nerf => $"{coloredLabel} <color=#EF4444>▼</color>",
                EffectChangeState.Mixed => $"{coloredLabel} <color=#FACC15>■</color>",
                _ => coloredLabel,
            };
        }

        private static string BuildEffectDataSummary(
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

            var lines = new List<string>();

            foreach (var effectType in allTypes)
            {
                if (effectType == EffectType.Normal) continue;

                bool viewingHas = viewingEffects != null && viewingEffects.TryGetValue(effectType, out _)
                                  && Utils.HasEffectType(viewingEffectType, effectType);
                bool equippedHas = equippedEffects != null && equippedEffects.TryGetValue(effectType, out _)
                                   && Utils.HasEffectType(equippedEffectType, effectType);

                if (!viewingHas && !equippedHas) continue;

                var state = viewingHas && equippedHas ? EffectChangeState.None
                    : viewingHas ? EffectChangeState.Buff
                    : EffectChangeState.Nerf;
                lines.Add(FormatEffectLine(effectType, state));
            }

            if (lines.Count == 0)
                return string.Empty;

            return "Magic Effect\n" + string.Join("\n", lines);
        }

        private static string BuildEffectGrowthSummary(
            EffectGrowth[] viewingEffects,
            EffectGrowth[] equippedEffects)
        {
            var allEffectTypes = new HashSet<EffectType>();
            if (viewingEffects != null)
                foreach (var e in viewingEffects) allEffectTypes.Add(e.effectType);
            if (equippedEffects != null)
                foreach (var e in equippedEffects) allEffectTypes.Add(e.effectType);

            var lines = new List<string>();

            foreach (var effectType in allEffectTypes)
            {
                if (effectType == EffectType.Normal) continue;

                bool viewingHas = viewingEffects != null && System.Array.Exists(viewingEffects, e => e.effectType == effectType);
                bool equippedHas = equippedEffects != null && System.Array.Exists(equippedEffects, e => e.effectType == effectType);
                if (!viewingHas && !equippedHas) continue;

                var vg = FindEffectGrowth(viewingEffects, effectType);
                var eqg = FindEffectGrowth(equippedEffects, effectType);
                var state = GetEffectGrowthChangeState(vg, eqg, viewingHas, equippedHas);
                lines.Add(FormatEffectLine(effectType, state));
            }

            if (lines.Count == 0)
                return string.Empty;

            return "Magic Effect\n" + string.Join("\n", lines);
        }

        /// <summary> 둘 다 가진 이펙트는 세부 성장치(duration/value/dot)를 비교해 일부만 증가+일부만 감소면 Mixed로 판정한다. </summary>
        private static EffectChangeState GetEffectGrowthChangeState(EffectGrowth a, EffectGrowth b, bool viewingHas, bool equippedHas)
        {
            if (viewingHas && !equippedHas) return EffectChangeState.Buff;
            if (!viewingHas && equippedHas) return EffectChangeState.Nerf;

            bool anyIncrease = false;
            bool anyDecrease = false;

            void Check(float av, float bv)
            {
                float diff = av - bv;
                if (diff > 0.001f) anyIncrease = true;
                else if (diff < -0.001f) anyDecrease = true;
            }

            Check(a.durationGrowth, b.durationGrowth);
            Check(a.valueGrowth, b.valueGrowth);
            Check(a.dotDamageGrowth, b.dotDamageGrowth);

            if (anyIncrease && anyDecrease) return EffectChangeState.Mixed;
            if (anyIncrease) return EffectChangeState.Buff;
            if (anyDecrease) return EffectChangeState.Nerf;
            return EffectChangeState.None;
        }

        private static EffectGrowth FindEffectGrowth(EffectGrowth[] arr, EffectType type)
        {
            if (arr == null) return default;
            foreach (var e in arr)
                if (e.effectType == type) return e;
            return default;
        }
    }
}
