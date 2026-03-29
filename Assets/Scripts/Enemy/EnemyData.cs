using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enemy
{
    [System.Serializable]
    public class EnemyBase
    {
        public int hp;
        public int atk;
        public float moveSpeed;
        public int armor;
        public int magicResistance;
    }

    [System.Serializable]
    public class ShootingData
    {
        public int projectileAtk;
        public float projectileSpeed;
    }

    [System.Serializable]
    public class FlyingShootingData
    {
        public int flyingProjectileAtk;
        public float flyingProjectileSpeed;
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;

        public EnemyBase @base;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<EnemyTag> enemyTags;

        public ShootingData shooter;
        public FlyingShootingData flyingShooter;
    }

}