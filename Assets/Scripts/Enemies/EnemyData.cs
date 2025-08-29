using System.Collections.Generic;

[System.Serializable]
public class EnemyBase
{
    public int hp;
    public int atk;
    public float moveSpeed;
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
    public string enemyName;      // 디버그/가독성용 이름

    public EnemyBase @base;
    public List<string> tags;

    public ShootingData shooter;
    public FlyingShootingData flyingShooter;
}

[System.Serializable]
public class EnemyDataWrapper
{
    public List<EnemyData> enemies;
}
