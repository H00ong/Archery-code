using UnityEngine;

public class MoveSpeedIncrease : PlayerSkillModuleBase, IPlayerUpgrader
{
    [SerializeField] float modifier = .2f;
    private float MoveSpeedModifier 
    {
        get => Level * modifier;
    }

    public override void UpdateSkill()
    {
        base.UpdateSkill();

        Apply();
    }

    public override void Init(PlayerSkill skill)
    {
        base.Init(skill);

        Apply();
    }

    public void Apply()
    {
        playerSkill.UpgradeMoveSpeed(MoveSpeedModifier);
    }
}
