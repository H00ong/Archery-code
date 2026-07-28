using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// Poison 속성 강화 스킬 — 플레이어의 Poison EffectData에 dotDamage(및 duration)를 버프한다.
    /// PlayerStat의 InGame Buff Layer에 누적되므로, 이 값이 오브·베럴·총알의 Poison 데미지에 자동 반영된다.
    /// </summary>
    public class PoisonEnhance : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float dotDamagePerLevel = 0.5f;
        [SerializeField, Min(0f)] private float durationPerLevel = 0.5f;
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

            var buff = new EffectData(
                duration: durationPerLevel * Level,
                value: 0f,
                dotDamage: dotDamagePerLevel * Level,
                tickInterval: tickIntervalBase
            );

            stat.SetBuffEffectData(EffectType.Poison, buff);
        }
    }
}
