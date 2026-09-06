using Players;
using Players.SkillModule;
using UnityEngine;

public class AttackPowerIncrease : PlayerSkillModuleBase, IPlayerUpgrader
{
    [SerializeField, Min(1)] private int attackPowerPerLevel = 1;

    public override void Init(PlayerSkill skill)
    {
        base.Init(skill);
        Apply();
    }

    public override void UpdateSkill()
    {
        base.UpdateSkill();
        Apply();
    }

    public void Apply()
    {
        playerSkill.UpgradeAttackPower(attackPowerPerLevel * Level);
    }
}