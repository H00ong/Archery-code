using System;

namespace Players
{
    /// <summary>
    /// CharacterSO에서 사용하는 캐릭터 기본 능력치 데이터.
    /// PlayerStat의 Base Layer에 적용된다.
    /// </summary>
    [Serializable]
    public struct CharacterBaseStatData
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
