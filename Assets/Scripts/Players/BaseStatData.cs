using System;

namespace Players
{
    /// <summary>
    /// 캐릭터·장비 공용 기본 능력치 구조체.
    /// CharacterIdentity와 EquipmentIdentity의 baseStat 필드에 사용된다.
    /// </summary>
    [Serializable]
    public struct BaseStatData
    {
        public int maxHP;
        public int attackPower;
        public float moveSpeed;
        public int armor;
        public int magicResistance;
        public float attackSpeed;
        public float projectileSpeed;
        public EffectType attackEffectType;
    }
}
