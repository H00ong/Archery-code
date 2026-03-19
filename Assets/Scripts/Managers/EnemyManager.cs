using System;
using System.Collections.Generic;
using UnityEngine;
using Enemy;
using Game.Stage.Management;
using Map;
using UnityEngine.AddressableAssets;

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
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        void OnEnable()
        {
            EventBus.Subscribe(EventType.TransitionToLobby, ClearEnemiesForMapClear);
            EventBus.Subscribe(EventType.Retry, ClearEnemiesForMapClear);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(EventType.TransitionToLobby, ClearEnemiesForMapClear);
            EventBus.Unsubscribe(EventType.Retry, ClearEnemiesForMapClear);
        }

        #region Stat Caching
        public void SetEnemyStat(string name, EnemyTag tag, EnemyStat stat)
        {
            var enemyData = DataManager.Instance.GetEnemyData(name, tag);
            var currentMapData = MapManager.Instance.CurrentMapData;
            var currentStageIndex = StageManager.Instance.CurrentStageIndex;

            EnemyStatUtil.CalculateStat(
                stat, enemyData, tag, currentMapData, currentStageIndex);

            // 디버그 로그: 최종 스탯 확인
            Debug.Log($"[EnemyManager] GetStat for {name} | Tag: {tag}\n" +
                    $"  Base - HP: {stat.MaxHP}, ATK: {stat.AttackPower}, MoveSpeed: {stat.MoveSpeed}\n" +
                    $"  Shooting - ProjectileATK: {stat.shooting.projectileAtk}, ProjectileSpeed: {stat.shooting.projectileSpeed}\n" +
                    $"  FlyingShooting - FlyingProjectileATK: {stat.flyingShooting.flyingProjectileAtk}, FlyingProjectileSpeed: {stat.flyingShooting.flyingProjectileSpeed}");
        }

        #endregion

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

        #region Spawn Helpers

        private async Awaitable SpawnSingleEnemyAsync(
            AssetReferenceGameObject enemyRef,
            Vector3 position,
            EnemyIdentity identity,
            System.Action onSpawned)
        {
            var pool = PoolManager.Instance;

            if (!pool.TryGetObject(enemyRef, out var enemyObj, pool.enemyPool))
                enemyObj = await pool.GetObjectAsync(enemyRef, pool.enemyPool);

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (!enemyObj)
                throw new InvalidOperationException($"[EnemyManager] Failed to get enemy from pool: {enemyRef}");

            var controller = enemyObj.GetComponent<EnemyController>();
            if (!controller)
                throw new InvalidOperationException($"[EnemyManager] EnemyController not found on spawned object: {enemyObj.name}");

            controller.transform.position = position;
            onSpawned?.Invoke();
            enemyObj.SetActive(true);
            controller.InitializeEnemy(identity);
            Enemies.Add(controller);
        }

        #endregion
        
        public async Awaitable SpawnBossEnemyAsync(int bossStageIndex)
        {
            try
            {
                var mapManager = MapManager.Instance;
                var bossIdentity = mapManager.GetBossIdentity(bossStageIndex);
                var bossRef = bossIdentity.Prefab;
                var spawnPoint = mapManager.GetBossSpawnPoint();

                await SpawnSingleEnemyAsync(bossRef, spawnPoint.position, bossIdentity, () => spawnPoint.gameObject.SetActive(true));
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[EnemyManager] SpawnBossEnemyAsync canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnemyManager] SpawnBossEnemyAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Awaitable SpawnEnemyAsync(int count)
        {
            try
            {
                var mapManager = MapManager.Instance;
                var spawnPoints = mapManager.GetEnemySpawnPoints(count);

                int availableCount = spawnPoints.Count;

                for (int i = 0; i < availableCount; i++)
                {
                    var identity = mapManager.GetEnemyIdentity();
                    var enemyRef = identity.Prefab;
                    var spawnPoint = spawnPoints[i];

                    await SpawnSingleEnemyAsync(
                        enemyRef,
                        spawnPoint.position,
                        identity,
                        () => spawnPoint.gameObject.SetActive(true));
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[EnemyManager] SpawnEnemyAsync canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnemyManager] SpawnEnemyAsync failed: {ex.Message}");
                throw;
            }
        }

        public void RemoveEnemy(EnemyController enemy)
        {
            if (!Enemies.Contains(enemy))
            {
                Debug.LogError("Remove enemy Error");
                return;
            }

            Enemies.Remove(enemy);

            if (Enemies.Count <= 0)
            {
                StageManager.Instance.HandleCommand(StageCommandType.AllEnemiesDefeated);
            }
        }

        public void ClearEnemiesForMapClear()
        {
            if(Enemies.Count <= 0)
                return;

            foreach (var enemy in Enemies)
            {
                if (enemy != null)
                {
                    enemy.ReturnImmediately();
                }
            }

            Enemies.Clear();
        }
    }
}