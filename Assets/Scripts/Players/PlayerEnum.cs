namespace Players
{
    public enum CharacterName
    {
        BlueWizard,
        // 캐릭터 추가 시 여기에 enum 값 추가
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

