namespace Enemies
{
    public enum EnemyState
    {
        Idle,
        Attack,
        Move,
        Dead,
        Hurt
    }

    public enum EnemyName
    {
        None,
        Slime,
        Bird,
        Octopus,
        Box,
        TurtleShell,
        SkeletonMinion
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
    }
}