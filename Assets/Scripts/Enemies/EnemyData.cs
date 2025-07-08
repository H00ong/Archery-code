using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public float moveSpeed;
}

[System.Serializable]
public class EnemyDataList
{
    public EnemyData[] enemies;
}
