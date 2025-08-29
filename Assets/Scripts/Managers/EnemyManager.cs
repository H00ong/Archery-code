using Game.Enemies.Enum;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static List<EnemyController> enemies = new List<EnemyController>();

    public static void ClearAllEnemies()
    {
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        enemies.Clear();
    }

    public static void EnemySpawn(GameObject _go, MapData _mapData, int _stageIndex) 
    {
        EnemyController enemy = _go.GetComponent<EnemyController>();
        if (enemy == null) return;

        enemy.InitializeEnemy(_mapData, _stageIndex);
        enemies.Add(enemy);
    }

    public static void RemoveEnemy(GameObject _go)
    {
        EnemyController _enemy = _go.GetComponent<EnemyController>();
        if (_enemy == null) return;

        if (!enemies.Contains(_enemy))
        {
            Debug.LogError("Remove enemy Error");
            return;
        }
        else 
        {
            enemies.Remove(_enemy);
        }
    }
}
