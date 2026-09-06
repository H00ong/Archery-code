using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 기본 공격에 Poison 속성을 부여하는 스킬 (maxLevel 1).
    /// 캐릭터·장비에 이미 Poison이 있으면 SkillDefinition의 등장 조건에서 후보에 오르지 않는다.
    /// </summary>
    public class PoisonAttackGrant : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float baseDuration = 3f;
        [SerializeField, Min(0f)] private float baseDotDamage = 0.8f;
        [SerializeField, Min(0.05f)] private float baseTickInterval = 0.4f;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public void Apply()
        {
            var stat = PlayerController.Instance != null ? PlayerController.Instance.Stat : null;
            if (stat == null) return;

            stat.SetBuffAttackEffectType(EffectType.Poison);

            // 속성과 함께 기본 중독 수치를 깔아둔다. PoisonEnhance는 별도 source로 이 위에 누적된다.
            stat.SetBuffEffectData(EffectType.Poison, GetType().Name, new EffectData(
                duration: baseDuration,
                value: 0f,
                dotDamage: baseDotDamage,
                tickInterval: baseTickInterval
            ));
        }
    }
}
