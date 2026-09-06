using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 관통 화살 — 화살이 적을 뚫고 지나간다. 레벨당 관통 가능한 적이 1명씩 늘어난다.
    /// </summary>
    public class PierceShot : PlayerSkillModuleBase, IBulletUpgrader
    {
        [SerializeField, Min(1)] private int pierceCountPerLevel = 1;

        public int PierceCount => pierceCountPerLevel * Level;

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

        // 실제 값은 PlayerAttack이 발사 시점에 PierceCount를 읽어 적용한다.
        public void Apply() { }
    }
}
