using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 유도 화살 — 발사된 화살이 가장 가까운 적을 향해 궤도를 수정한다.
    /// 레벨이 오를수록 선회 속도가 빨라져 더 급격하게 추적한다.
    /// </summary>
    public class HomingShot : PlayerSkillModuleBase, IBulletUpgrader
    {
        [SerializeField, Min(0f)] private float turnSpeedPerLevel = 120f;

        public float TurnSpeed => turnSpeedPerLevel * Level;

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

        // 실제 값은 PlayerAttack이 발사 시점에 TurnSpeed를 읽어 적용한다.
        public void Apply() { }
    }
}
