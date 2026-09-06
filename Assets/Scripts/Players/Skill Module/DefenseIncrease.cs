using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 방어 강화 스킬 — 물리 방어력(Armor)과 마법 저항력(MagicResistance)을 동시에 올린다.
    /// Armor는 피격 데미지를, MagicResistance는 도트 데미지를 각각 경감한다.
    /// </summary>
    public class DefenseIncrease : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0)] private int armorPerLevel = 5;
        [SerializeField, Min(0)] private int magicResistancePerLevel = 5;

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

            stat.SetBuffArmor(armorPerLevel * Level);
            stat.SetBuffMagicResistance(magicResistancePerLevel * Level);
        }
    }
}
