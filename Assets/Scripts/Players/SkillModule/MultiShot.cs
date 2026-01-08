using Players.SkillModule;
using UnityEngine;

public class MultiShot : PlayerSkillModuleBase
{
    public int MultiShotCount
    {
        get => Level + 1;
    }
}
