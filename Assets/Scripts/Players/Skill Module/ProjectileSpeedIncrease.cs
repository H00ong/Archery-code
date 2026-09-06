using Players;
using Players.SkillModule;
using UnityEngine;

public class ProjectileSpeedIncrease : PlayerSkillModuleBase, IPlayerUpgrader
{
    [SerializeField, Min(0.1f)] private float projectileSpeedPerLevel = 2f;

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
        playerSkill.UpgradeProjectileSpeed(projectileSpeedPerLevel * Level);
    }
}