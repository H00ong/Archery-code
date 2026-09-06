using Managers;
using Players;
using Players.SkillModule;

public class FireBarrel : PlayerSkillModuleBase, IPickupBarrel
{
    private BarrelManager _barrelManager;
    private EffectType _type = EffectType.Fire;
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
