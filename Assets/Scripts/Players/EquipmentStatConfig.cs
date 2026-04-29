using System;
using Stat;

namespace Players
{
    [Serializable]
    public struct EquipmentEffectConfig
    {
        public EffectType effectType;
        public EffectData baseEffect;
    }

    [Serializable]
    public struct EquipmentEffectGrowth
    {
        public EffectType effectType;
        public float durationGrowth;
        public float valueGrowth;
        public float dotDamageGrowth;
        public float tickIntervalGrowth;
    }

    [Serializable]
    public struct EquipmentLevelStatGrowth
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
