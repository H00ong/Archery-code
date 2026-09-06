using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// Lightning 속성 강화 스킬 — Lightning EffectData의 duration(기절 시간)을 버프한다.
    /// LightningAttackGrant가 심어둔 Base duration(1초) 위에 레벨당 증분이 누적되는 방식이다.
    /// PlayerStat의 InGame Buff Layer에 누적되므로 기본 공격·오브·베럴·GroundZone에 모두 자동 반영된다.
    /// </summary>
    public class LightningEnhance : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float stunDurationPerLevel = 0.25f;

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

            // 기본 공격에 Lightning 속성이 아직 없어도 버프는 누적해 둔다.
            var buff = new EffectData(
                duration: stunDurationPerLevel * Level,
                value: 0f,
                dotDamage: 0f,
                tickInterval: 0f
            );

            stat.SetBuffEffectData(EffectType.Lightning, GetType().Name, buff);
        }
    }
}
