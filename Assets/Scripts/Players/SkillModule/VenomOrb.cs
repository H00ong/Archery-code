using Players;
using Players.SkillModule;

public class VenomOrb : PlayerSkillModuleBase, IOrbGenerator
{
    OrbManager orbManager;
    EffectType type = EffectType.Poison;
    int OrbCount => 2 * Level;

    public override void Init(PlayerSkill _skill)
    {
        base.Init(_skill);

        orbManager = FindAnyObjectByType<OrbManager>();

        GenerateOrb();
    }

    public void GenerateOrb()
    {
        orbManager.GenerateOrb(type, OrbCount);
    }

    public override void UpdateSkill()
    {
        base.UpdateSkill();
        
        orbManager.GenerateOrb(type, OrbCount);
    }
}
