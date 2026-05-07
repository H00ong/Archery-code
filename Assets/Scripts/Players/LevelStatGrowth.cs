using System;

namespace Players
{
    /// <summary>
    /// 캐릭터·장비 공용 레벨당 스탯 성장치 구조체.
    /// CharacterIdentity와 EquipmentIdentity의 levelStatGrowth 필드에 사용된다.
    /// </summary>
    [Serializable]
    public struct LevelStatGrowth
    {
        public int maxHP;
        public int attackPower;
        public float moveSpeed;
        public int armor;
        public int magicResistance;
        public float attackSpeed;
        public float projectileSpeed;
    }
}
