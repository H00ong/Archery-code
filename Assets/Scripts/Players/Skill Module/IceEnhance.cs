using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// Ice 속성 강화 스킬 — 플레이어의 Ice EffectData에 duration(및 감속 강도 value)을 버프한다.
    /// PlayerStat의 InGame Buff Layer에 누적되므로 오브·베럴·총알의 Ice 지속시간·감속률에 자동 반영된다.
    /// </summary>
    public class IceEnhance : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float durationPerLevel = 0.5f;
        [SerializeField, Min(0f)] private float slowValuePerLevel = 0.1f;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();
            Apply();
        }

        public void Apply()
        {
            var stat = PlayerController.Instance != null ? PlayerController.Instance.Stat : null;
            if (stat == null) return;

            var buff = new EffectData(
                duration: durationPerLevel * Level,
                value: slowValuePerLevel * Level,
                dotDamage: 0f,
                tickInterval: 0f
            );

            stat.SetBuffEffectData(EffectType.Ice, buff);
        }
    }
}
