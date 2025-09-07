using Game.Player.Attack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkillModuleBase : MonoBehaviour
{
    public int level;
    
    public void LevelUp() 
    {
        level++;
    }

    public virtual void Init() 
    {
        
    }
}

public interface IAttackSpeedModifier { void Apply(); }
public interface IMoveSpeedModifier { void Apply(); }
public interface IShootModifier { public void AddBullet(List<ShotInstruction> bullet); };

