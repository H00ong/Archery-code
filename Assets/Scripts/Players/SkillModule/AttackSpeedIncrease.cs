using Players;
using Players.SkillModule;
using UnityEngine;

public class AttackSpeedIncrease : PlayerSkillModuleBase, IPlayerUpgrader
{
    [SerializeField] float modifier = .2f;

    private float AttackSpeedModifier 
    {
        get => Level * modifier;
    }

    public override void Init(PlayerSkill _skill)
    {
        base.Init(_skill);

        Apply();
    }

    public void Apply()
    {
        playerSkill.UpgradeAttackSpeed(AttackSpeedModifier);
    }
}
