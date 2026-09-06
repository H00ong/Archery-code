using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 반사 화살 — 화살이 장애물에 부딪히면 사라지지 않고 튕겨 나간다.
    /// 반사 직후에는 이미 맞춘 적도 다시 타격할 수 있다. 레벨당 반사 횟수가 1회씩 늘어난다.
    /// </summary>
    public class ReflectShot : PlayerSkillModuleBase, IBulletUpgrader
    {
        [SerializeField, Min(1)] private int reflectCountPerLevel = 1;

        public int ReflectCount => reflectCountPerLevel * Level;

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

        // 실제 값은 PlayerAttack이 발사 시점에 ReflectCount를 읽어 적용한다.
        public void Apply() { }
    }
}
