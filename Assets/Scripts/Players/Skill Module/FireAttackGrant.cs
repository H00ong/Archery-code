using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 기본 공격에 Fire 속성을 부여하는 스킬 (maxLevel 1).
    /// 캐릭터·장비에 이미 Fire가 있으면 SkillDefinition의 등장 조건에서 후보에 오르지 않는다.
    /// </summary>
    public class FireAttackGrant : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float baseDuration = 2f;
        [SerializeField, Min(0f)] private float baseDotDamage = 1f;
        [SerializeField, Min(0.05f)] private float baseTickInterval = 0.3f;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public void Apply()
        {
            var stat = PlayerController.Instance != null ? PlayerController.Instance.Stat : null;
            if (stat == null) return;

            stat.SetBuffAttackEffectType(EffectType.Fire);

            // 속성과 함께 기본 화상 수치를 깔아둔다. FireEnhance는 별도 source로 이 위에 누적된다.
            stat.SetBuffEffectData(EffectType.Fire, GetType().Name, new EffectData(
                duration: baseDuration,
                value: 0f,
                dotDamage: baseDotDamage,
                tickInterval: baseTickInterval
            ));
        }
    }
}
