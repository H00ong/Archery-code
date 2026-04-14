using System;

namespace Players
{
    [Serializable]
    // TODO : effectData를 포함하는 형태로 변경 필요 (ex. 화상 효과 추가)
    public struct CharacterLevelStatGrowth
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
