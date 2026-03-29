using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[System.Serializable]
public class EffectConfig
{
    [JsonConverter(typeof(StringEnumConverter))]
    public EffectType effectType;
    public float duration;
    public float damagePerTick;
    public float tickInterval;
    public float effectValue;   // 슬로우 비율 등
}

namespace Map
{
    [System.Serializable]
    public class MapData
    {
        public string mapId;
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