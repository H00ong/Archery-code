using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Enemy;
using Game.Stage.Management;

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
                        var key = new EnemyKey(data.targetName, data.linkedTag);

                        if (!_globalModuleData.TryAdd(key, data))
                        {
                            Debug.LogWarning($"[EnemyManager] Duplicate Key: {key}");
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
            yield return new WaitUntil(() => _isModuleLoaded);

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
            
            // 데이터가 이미 로드되어 있으므로 InitializeEnemy 내부에서 GetModuleData 호출 시 문제 없음
            controller.InitializeEnemy(); 

            Enemies.Add(controller);
        }

        public IEnumerator SpawnEnemy(int count)
        {
            yield return new WaitUntil(() => _isModuleLoaded);

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

            Enemies.Remove(enemy);

            PoolManager.Instance.ReturnObject(enemy.gameObject);

            if (Enemies.Count <= 0)
            {
                StageManager.Instance.HandleCommand(StageCommandType.AllEnemiesDefeated);
            }
        }
    }
}