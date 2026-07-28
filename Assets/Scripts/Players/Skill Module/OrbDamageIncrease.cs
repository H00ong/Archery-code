using Managers;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 모든 오브의 "기본 데미지 배수(damageModifier)"를 증가시키는 스킬.
    /// 속성 값(EffectData)에는 관여하지 않는다.
    /// 레벨당 damageBonusPerLevel 만큼 damageModifier가 가산된다.
    /// (예: level 3 &amp; bonus 0.3 → +0.9 → 기본 1.0 → 최종 1.9배 데미지)
    /// </summary>
    public class OrbDamageIncrease : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0f)] private float damageBonusPerLevel = 0.3f;

        private OrbManager _orbManager;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            _orbManager = OrbManager.Instance != null ? OrbManager.Instance : FindAnyObjectByType<OrbManager>();
            Apply();
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();
            Apply();
        }

        public void Apply()
        {
            if (_orbManager == null) return;
            float bonus = damageBonusPerLevel * Level;
            _orbManager.SetOrbDamageBonus(bonus);
        }
    }
}
