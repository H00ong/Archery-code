using Game.Player;
using UnityEngine;

public class IceOrb : PlayerSkillModuleBase, IOrbGenerator
{
    OrbManager orbManager;
    OrbType type = OrbType.Ice;
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
        orbManager.GenerateOrb(type, OrbCount);
    }
}
