using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// Fire 속성 강화 스킬 — 플레이어의 Fire EffectData에 dotDamage(및 duration)를 버프한다.
    /// PlayerStat의 InGame Buff Layer에 누적되므로 기본 공격·Fire 오브·Fire 베럴·GroundZone에 모두 자동 반영된다.
    /// </summary>
    public class FireEnhance : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float dotDamagePerLevel = 0.3f;
        [SerializeField, Min(0f)] private float durationPerLevel = 0.35f;
        [SerializeField, Min(0f)] private float tickIntervalBase = 0.3f;

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

            // 기본 공격에 Fire 속성이 아직 없어도 버프는 그대로 누적해 둔다.
            // 이후 FireAttackGrant로 Fire가 부여되는 순간 누적분이 한 번에 적용된다.
            var buff = new EffectData(
                duration: durationPerLevel * Level,
                value: 0f,
                dotDamage: dotDamagePerLevel * Level,
                tickInterval: tickIntervalBase
            );

            stat.SetBuffEffectData(EffectType.Fire, GetType().Name, buff);
        }
    }
}
