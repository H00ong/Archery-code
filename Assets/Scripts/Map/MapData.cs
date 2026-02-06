using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class EffectConfig
{
    public EffectType effectType;
    public float duration;
    public float damagePerTick;
    public float tickInterval;
    public float effectValue;   // 슬로우 비율 등
}

namespace Map
{
    public enum MapType
    {
        Dungeon = 1,
        Jungle = 2,
        Desert = 3
    }

    [System.Serializable]
    public class MapDataWrapper
    {
        public List<MapData> maps;
    }

    [System.Serializable]
    public class MapData
    {
        public int mapType;
        public int stageCount;
        public MapModifiers mapModifiers;
        public StageGrowth stageGrowth;
        public List<int> enemyCountGrid;
        public List<EffectConfig> enemyEffects;
    }

    [System.Serializable]
    public class MapModifiers
    {
        public float meleeEnemyHpMulPerMap;
        public float rangedEnemyHpMulPerMap;
        public float bossEnemyHpMulPerMap;

        public float atkMulPerMap;
        public float moveSpeedMul;
        public float projectileSpeedMul;
        public float flyingProjectileSpeedMul;

        public float bossAtkMulPerMap;
        public float bossMoveSpeedMulPerMap;
        public float bossProjectileSpeedMulPerMap;
        public float bossFlyingProjectileSpeedMulPerMap;
    }

    [System.Serializable]
    public class StageGrowth
    {
        public float hpMulPerStage;
        public float atkMulPerStage;

        public float bossHpMulPerStage;
        public float bossAtkMulPerStage;
    }
}