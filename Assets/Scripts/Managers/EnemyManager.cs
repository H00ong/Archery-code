using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemy;
using Game.Stage.Management;
using UnityEngine;



namespace Managers
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        public List<EnemyController> Enemies = new List<EnemyController>();

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        public void ClearAllEnemies()
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

        public IEnumerator SpawnBossEnemey(int index)
        {
            MapManager mapManager = MapManager.Instance;
            PoolManager pool = PoolManager.Instance;
            
            var bossRef = mapManager.GetBossAssetRef(index);

            if (!pool.TryGetObject(bossRef, out var boss, pool.EnemyPool))
                yield return pool.GetObject(bossRef, inst => boss = inst, pool.EnemyPool);

            if (!boss)
            {
                Debug.LogError("Boss is null");
                yield break;
            }
            var spawnPoint = mapManager.GetBossSpawnPoint();

            boss.transform.position = spawnPoint.position;
            boss.SetActive(true);

            var controller = boss.GetComponent<EnemyController>();
            controller.InitializeEnemy();

            Enemies.Add(controller);
        }

        public IEnumerator SpawnEnemy(int count)
        {
            MapManager mapManager = MapManager.Instance;
            PoolManager pool = PoolManager.Instance;
            
            var list = new List<EnemyController>();
            var spawnPoints = mapManager.GetEnemySpawnPoint(count);

            for (int i = 0; i < count; i++)
            {
                var enemyRef = mapManager.GetEnemeyAssetRef();

                if (!pool.TryGetObject(enemyRef, out var enemy, pool.EnemyPool))
                    yield return pool.GetObject(enemyRef, inst => enemy = inst, pool.EnemyPool);

                var controller = enemy.GetComponent<EnemyController>();
                controller.transform.position = spawnPoints[i].position;

                list.Add(controller);
            }

            foreach (var enemy in list)
            {
                enemy.gameObject.SetActive(true);
                enemy.InitializeEnemy();

                Enemies.Add(enemy);
            }
        }

        public void RemoveEnemy(EnemyController enemy)
        {
            if (!Enemies.Contains(enemy))
            {
                Debug.LogError("Remove enemy Error");
                return;
            }
            else
            {
                Enemies.Remove(enemy);
                
                PoolManager.Instance.ReturnObject(enemy.gameObject);

                if (Enemies.Count <= 0)
                {
                    StageManager.Instance.HandleCommand(StageCommandType.AllEnemiesDefeated);
                }
            }
        }
    }
}
