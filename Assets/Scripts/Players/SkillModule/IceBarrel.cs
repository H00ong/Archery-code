using Game.Player;
using Managers;
using Players;
using Players.SkillModule;
using UnityEngine;

public class IceBarrel : PlayerSkillModuleBase, IPickupBarrel
{
    private BarrelManager _barrelManager;
    private BarrelType _type = BarrelType.Ice;
    private int BarrelCount => Level;

    public override void Init(PlayerSkill _skill)
    {
        base.Init(_skill);

        _barrelManager = FindAnyObjectByType<BarrelManager>();
        GenerateActivate();
    }

    public override void UpdateSkill()
    {
        base.UpdateSkill();

        GenerateActivate();
    }

    public void GenerateActivate()
    {
        _barrelManager.UpdateBarrelSkill(_type, BarrelCount);
    }
}
