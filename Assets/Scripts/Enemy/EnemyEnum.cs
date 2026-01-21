namespace Enemy
{
    public enum EnemyState
    {
        Idle,
        Attack,
        Move,
        Dead,
        Hurt
    }
    
    public enum EnemyPointType
    {
        FlyingShootingMuzzle,
        NormalShootingMuzzle,
        PatrolPoint
    }

    public enum EnemyName
    {
        None,
        Slime,
        Bird,
        Octopus,
        Box,
        Cactus,
        TurtleShell,
        SkeletonMinion,
        SkeletonWarrior,
        MeleeGolem,
        RangedGolem,
        Mushroom,
        Orc,
        Stingray,

    }

    [System.Flags]
    public enum EnemyTag : int
    {
        None = 0,

        // Identity
        Melee = 1 << 0,
        Ranged = 1 << 1,
        Boss = 1 << 2,

        // Move
        FollowMove = 1 << 3,
        PatternMove = 1 << 4,
        RandomMove = 1 << 5,

        // Attack
        MeleeAttack = 1 << 6,
        Shoot = 1 << 7,
        FlyingShoot = 1 << 8,
        FollowMeleeAttack = 1 << 9,
        
        // Attributes
        Fire = 1 << 26,
        Ice = 1 << 27,
        Poison = 1 << 28,
        
        MoveMask = FollowMove | PatternMove | RandomMove,
        AttackMask = MeleeAttack | Shoot | FlyingShoot | FollowMeleeAttack,
        AttributeMask = Fire | Ice | Poison,
    }
}