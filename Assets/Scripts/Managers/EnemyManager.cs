using Game.Enemies.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] StageManager stageManager;

    public static List<EnemyController> Enemies = new List<EnemyController>();
    public static event Action OnAllEnemiesSpawned;

    public static void ClearAllEnemies()
    {
        foreach (EnemyController enemy in Enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        Enemies.Clear();
    }

    public static void EnemySpawn(GameObject _go, MapData _mapData, int _stageIndex) 
    {
        EnemyController enemy = _go.GetComponent<EnemyController>();
        if (enemy == null) return;

        enemy.InitializeEnemy(_mapData, _stageIndex);
        Enemies.Add(enemy);
    }

    public static void AllEnemiesSpawned() 
    {
        OnAllEnemiesSpawned?.Invoke();
    }

    public static void RemoveEnemy(EnemyController _enemy)
    {
        if (!Enemies.Contains(_enemy))
        {
            Debug.LogError("Remove enemy Error");
            return;
        }
        else 
        {
            Enemies.Remove(_enemy);

            if (Enemies.Count <= 0) 
            {
                StageManager.ChangeStageState(StageState.Clear);
                return;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(stageManager == null) stageManager = FindAnyObjectByType<StageManager>();
    }
#endif
}
