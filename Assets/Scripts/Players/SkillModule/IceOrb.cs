using Game.Player;
using UnityEngine;

public class IceOrb : PlayerSkillModuleBase, IOrbGenerator
{
    OrbManager _orbManager;
    OrbType _type = OrbType.Ice;
    int OrbCount => 2 * Level;

    public override void Init(PlayerSkill _skill)
    {
        base.Init(_skill);

        _orbManager = FindAnyObjectByType<OrbManager>();

        GenerateOrb();
    }

    public void GenerateOrb()
    {
        _orbManager.GenerateOrb(_type, OrbCount);
    }

    public override void UpdateSkill()
    {
        _orbManager.GenerateOrb(_type, OrbCount);
    }
}
