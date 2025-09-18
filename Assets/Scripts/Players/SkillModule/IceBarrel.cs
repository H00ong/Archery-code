using Game.Player;
using UnityEngine;

public class IceBarrel : PlayerSkillModuleBase, IPickupBarrel
{
    BarrelManager _barrelManager;
    BarrelType _type = BarrelType.Ice;
    int BarrelCount => Level;

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
