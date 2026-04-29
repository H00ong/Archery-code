using System;

namespace Players
{
    [Serializable]
    public struct EquipmentBaseStatData
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
