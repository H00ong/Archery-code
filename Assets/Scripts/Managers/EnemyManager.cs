using System.Collections;
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
        public List<EnemyController> Enemies = new List<EnemyController>();

        [Header("Addressable Settings")]
        [SerializeField] private string moduleConfigLabel = "enemyModule_config"; 

        // [중앙 저장소] 태그를 키로 데이터를 보관
        private Dictionary<EnemyKey, BaseModuleData> _globalModuleData = new();
        
        // 로딩 상태 확인용 플래그 & 핸들
        private bool _isModuleLoaded = false;
        private AsyncOperationHandle<IList<BaseModuleData>> _loadHandle;

        // ── Flyweight 캐싱 ──
        // 맵에 설정된 효과 정보 (모든 적이 공유)
        private readonly Dictionary<EffectType, EffectConfig> _cachedMapEffects = new();

        private MapData _currentMapData;
        private int _currentStageIndex;

        public IReadOnlyDictionary<EffectType, EffectConfig> CachedMapEffects => _cachedMapEffects;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                StartCoroutine(LoadAllModules());
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            // 메모리 누수 방지를 위해 핸들 해제
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }
        
        private IEnumerator LoadAllModules()
        {
            _loadHandle = Addressables.LoadAssetsAsync<BaseModuleData>(moduleConfigLabel, null);
            yield return _loadHandle;

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _globalModuleData.Clear();

                foreach (var data in _loadHandle.Result)
                {
                    if (data)
                    {
                        var key = new EnemyKey(data.targetName, data.targetTag);

                        if (!_globalModuleData.TryAdd(key, data))
                        {
                            Debug.LogWarning($"[EnemyManager] Duplicate module key detected: {key}. Existing module: {_globalModuleData[key]}, New module: {data}");
                        }
                    }
                }
                
                _isModuleLoaded = true;
                Debug.Log($"[EnemyManager] Loaded {_globalModuleData.Count} modules.");
            }
        }
        
        public BaseModuleData GetModuleData(EnemyKey key)
        {
            return _globalModuleData.GetValueOrDefault(key);
        }

        #region Flyweight Caching

        /// <summary>
        /// 맵/스테이지 전환 시 호출. 맵 이펙트 캐싱 + 적 Stat 캐시 초기화.
        /// </summary>
        public void SetUpEnemyEffects(MapData mapData, int stageIndex)
        {
            _currentMapData = mapData;
            _currentStageIndex = stageIndex;

            _cachedMapEffects.Clear();

            if (mapData.enemyEffects != null)
            {
                foreach (var effect in mapData.enemyEffects)
                {
                    _cachedMapEffects[effect.effectType] = effect;
                }
            }
        }

        public EnemyStats GetStat(EnemyName name, EnemyTag tag)
        {
            var enemyData = DataManager.Instance.GetEnemyData(name, tag);
            if (enemyData == null)
            {
                Debug.LogWarning($"[EnemyManager] EnemyData not found: {name} | {tag}");
                return new EnemyStats();
            }

            var stats = EnemyStatUtil.CalculateStat(
                enemyData, tag, _currentMapData, _currentStageIndex, _cachedMapEffects);

            // 디버그 로그: 최종 스탯 확인
            Debug.Log($"[EnemyManager] GetStat for {name} | Tag: {tag}\n" +
                      $"  Base - HP: {stats.baseStats.hp}, ATK: {stats.baseStats.atk}, MoveSpeed: {stats.baseStats.moveSpeed}\n" +
                      $"  Shooting - ProjectileATK: {stats.shooting.projectileAtk}, ProjectileSpeed: {stats.shooting.projectileSpeed}\n" +
                      $"  FlyingShooting - FlyingProjectileATK: {stats.flyingShooting.flyingProjectileAtk}, FlyingProjectileSpeed: {stats.flyingShooting.flyingProjectileSpeed}\n" +
                      $"  OffensiveEffects Count: {stats.offensiveEffects?.Count ?? 0}");
            
            if (stats.offensiveEffects != null && stats.offensiveEffects.Count > 0)
            {
                foreach (var effect in stats.offensiveEffects)
                {
                    Debug.Log($"    Effect - Type: {effect.Key}, Duration: {effect.Value.duration}, " +
                              $"DamagePerTick: {effect.Value.damagePerTick}, TickInterval: {effect.Value.tickInterval}, " +
                              $"EffectValue: {effect.Value.effectValue}");
                }
            }

            return stats.Clone();
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

        /// <summary>
        /// 단일 적 스폰 헬퍼 (Pool에서 가져오기 + 위치 설정)
        /// </summary>
        private IEnumerator SpawnSingleEnemyAsync(
            AssetReferenceGameObject enemyRef, 
            Vector3 position,
            EnemyIdentity identity,
            System.Action<EnemyController> onSpawned)
        {
            var pool = PoolManager.Instance;

            if (!pool.TryGetObject(enemyRef, out var enemyObj, pool.EnemyPool))
                yield return pool.GetObject(enemyRef, inst => enemyObj = inst, pool.EnemyPool);

            if (!enemyObj)
            {
                Debug.LogError("[EnemyManager] Failed to spawn enemy");
                yield break;
            }

            var controller = enemyObj.GetComponent<EnemyController>();
            controller.transform.position = position;

            onSpawned?.Invoke(controller);

            enemyObj.SetActive(true);
            controller.InitializeEnemy(identity);
            Enemies.Add(controller);
        }

        #endregion
        
        public IEnumerator SpawnBossEnemey(int index)
        {
            yield return new WaitUntil(() => _isModuleLoaded);

            var mapManager = MapManager.Instance;
            var bossIdentity = mapManager.GetBossIdentity(index);
            var bossRef = bossIdentity != null ? bossIdentity.Prefab : mapManager.GetBossAssetRef(index);
            var spawnPoint = mapManager.GetBossSpawnPoint();

            yield return SpawnSingleEnemyAsync(bossRef, spawnPoint.position, bossIdentity, null);
        }

        public IEnumerator SpawnEnemy(int count)
        {
            yield return new WaitUntil(() => _isModuleLoaded);

            var mapManager = MapManager.Instance;
            var spawnPoints = mapManager.GetEnemySpawnPoint(count);
            var useIdentity = mapManager.HasEnemyIdentityList;

            for (int i = 0; i < count; i++)
            {
                EnemyIdentity identity = null;
                AssetReferenceGameObject enemyRef;

                if (useIdentity)
                {
                    identity = mapManager.GetEnemyIdentity();
                    enemyRef = identity.Prefab;
                }
                else
                {
                    enemyRef = mapManager.GetEnemeyAssetRef();
                }

                var spawnPoint = spawnPoints[i];
                yield return SpawnSingleEnemyAsync(
                    enemyRef, 
                    spawnPoint.position, 
                    identity, 
                    _ => spawnPoint.gameObject.SetActive(true));
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