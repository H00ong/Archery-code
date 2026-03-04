using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Enemy;
using Game.Stage.Management;
using Map;

namespace Managers
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        

        [Header("Addressable Settings")]
        [SerializeField] private string moduleConfigLabel = "enemy_module";

        private bool _isModuleLoaded = false;
        private AsyncOperationHandle<IList<BaseModuleData>> _loadHandle;

        public List<EnemyController> Enemies = new List<EnemyController>();
        private Dictionary<EnemyKey, BaseModuleData> _enemyModuleData = new();

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

        private void OnDestroy()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }
        
        public async Awaitable LoadEnemyModulesAsync()
        {
            try
            {
                _loadHandle = Addressables.LoadAssetsAsync<BaseModuleData>(moduleConfigLabel, null);
                await _loadHandle.Task;
                destroyCancellationToken.ThrowIfCancellationRequested();

                if (_loadHandle.Status != AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException($"[EnemyManager] Addressable load failed for label: {moduleConfigLabel}");

                _enemyModuleData.Clear();

                foreach (var data in _loadHandle.Result)
                {
                    if (data)
                    {
                        var key = new EnemyKey(data.targetName, data.targetTag);

                        if (!_enemyModuleData.TryAdd(key, data))
                        {
                            Debug.LogWarning($"[EnemyManager] Duplicate module key detected: {key}. Existing module: {_enemyModuleData[key]}, New module: {data}");
                        }
                    }
                }

                _isModuleLoaded = true;
                Debug.Log($"[EnemyManager] Loaded {_enemyModuleData.Count} modules.");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[EnemyManager] LoadEnemyModulesAsync canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnemyManager] LoadEnemyModulesAsync failed: {ex.Message}");
                throw;
            }
        }
        
        public BaseModuleData GetModuleData(EnemyKey key)
        {
            return _enemyModuleData.GetValueOrDefault(key);
        }

        #region Stat Caching
        public void SetEnemyStat(EnemyName name, EnemyTag tag, EnemyStat stat)
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
    }
}