using System;
using UnityEngine;

namespace Game.Player 
{
    public enum OrbType
    {
        Common,
        Venom,
        Blaze,
        Ice,
        Lightning,
    }

    public enum PlayerState
    {
        Idle,
        Move,
        Attack,
        Dead
    }

    
    public enum PlayerSkillId
    {
        DiagonalShot,
        HorizontalShot,
        MultiShot,
        MoveSpeedIncrease,
        AttackSpeedIncrease,
        VenomOrb,
        IceOrb,
    }


    [Flags]
    public enum SkillTag : uint
    {
        None = 0,
        Venom = 1 << 0,
        Blaze = 1 << 1,
        Ice = 1 << 2,
        Lightning = 1 << 3, // (Lightening พฦดิ)
        Orb = 1 << 4,
        Melee = 1 << 5,
        Ranged = 1 << 6,
    }

    public static class SkillTagUtil
    {
        public static bool HasAny(SkillTag v, SkillTag mask) => (v & mask) != 0;
    }


    public enum SkillRarity 
    {
        Common,
        Rare,
        Epic,
        Legend,
    }
}

