using Stat;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 기본 공격에 Lightning 속성을 부여하는 스킬 (maxLevel 1).
    /// 캐릭터·장비에 이미 Lightning이 있으면 SkillDefinition의 등장 조건에서 후보에 오르지 않는다.
    /// </summary>
    public class LightningAttackGrant : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float baseStunDuration = 1f;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public void Apply()
        {
            var stat = PlayerController.Instance != null ? PlayerController.Instance.Stat : null;
            if (stat == null) return;

            // 플래그만 켜면 LightningEnhance가 이미 쌓아 둔 버프가 그 즉시 함께 적용된다.
            stat.SetBuffAttackEffectType(EffectType.Lightning);

            // 속성과 함께 기본 기절 시간을 깔아둔다. LightningEnhance는 별도 source로 이 위에 누적된다.
            stat.SetBuffEffectData(EffectType.Lightning, GetType().Name, new EffectData(
                duration: baseStunDuration,
                value: 0f,
                dotDamage: 0f,
                tickInterval: 0f
            ));
        }
    }
}
