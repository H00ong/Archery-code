namespace Players
{
    public enum BarrelType
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
        IceBarrelGenerate,
    }

    public enum SkillRarity
    {
        Common,
        Rare,
        Epic,
        Legend,
    }
}

