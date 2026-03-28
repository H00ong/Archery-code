using System.Collections.Generic;
using Map;
using Players;
using Stat;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public sealed class BarrelConfig
    {
        public EffectType type;
        public int attackCountPerBarrel = 0;
        public float spawnTimer = 0f;
    }

    public class BarrelManager : MonoBehaviour
    {
        public static BarrelManager Instance { get; private set; }

        [SerializeField] private string addressableAssetLabel = "barrel_config";
        [SerializeField] private float barrelGenerateTime = 5f;
        [SerializeField] private float meteorDamageModifier = 1f;
        [SerializeField] private float meteorYOffset = 1f;

        private AsyncOperationHandle<IList<BarrelScriptable>> _handle;

        private PoolManager _poolManager;
        private EnemyManager _enemyManager;

        private Dictionary<EffectType, BarrelScriptable> _barrelSoDict = new();
        private readonly Dictionary<EffectType, BarrelConfig> _barrelConfigDict = new();
        private Queue<EffectType> _meteorTargetQueue = new();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            _poolManager = PoolManager.Instance;
            _enemyManager = EnemyManager.Instance;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.StageCombatStarted, MeteorAttack);
            EventBus.Subscribe(EventType.StageCleared, OnStageCleared);
            EventBus.Subscribe(EventType.MapCleared, OnMapCleared);
            EventBus.Subscribe(EventType.Retry, OnMapCleared);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.StageCombatStarted, MeteorAttack);
            EventBus.Unsubscribe(EventType.StageCleared, OnStageCleared);
            EventBus.Unsubscribe(EventType.MapCleared, OnMapCleared);
            EventBus.Unsubscribe(EventType.Retry, OnMapCleared);
        }

        private void OnStageCleared()
        {
            _meteorTargetQueue.Clear();

            foreach (var config in _barrelConfigDict.Values)
                config.spawnTimer = 0f;
        }

        private void OnMapCleared()
        {
            _meteorTargetQueue.Clear();
            _barrelConfigDict.Clear();
        }

        private void Update()
        {
            GenerateBarrelCheck();
        }

        public async Awaitable LoadBarrelAssetsAsync()
        {
            if (_barrelSoDict is { Count: > 0 })
                return;

            _barrelSoDict ??= new Dictionary<EffectType, BarrelScriptable>();

            _handle = Addressables.LoadAssetsAsync<BarrelScriptable>(
                addressableAssetLabel,
                so => _barrelSoDict[so.type] = so
            );

            await _handle.Task;

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new System.Exception($"Failed to load BarrelScriptable assets with label {addressableAssetLabel}");
            }
        }

        private void GenerateBarrelCheck()
        {
            if (!GameManager.Instance.IsInGame)
                return;

            if (StageManager.Instance != null && !StageManager.Instance.IsInCombat)
                return;

            foreach (var key in _barrelConfigDict.Keys)
            {
                var config = _barrelConfigDict[key];
                config.spawnTimer += Time.deltaTime;

                if (!(config.spawnTimer >= barrelGenerateTime))
                    continue;

                config.spawnTimer = 0f;
                GenerateBarrelAsync(config.type).Forget();
            }
        }

        private async Awaitable GenerateBarrelAsync(EffectType type)
        {
            GameObject go = null;
            var assetRef = _barrelSoDict[type].barrelPrefab;

            try
            {
                if (!_poolManager.TryGetObject(assetRef, out go, _poolManager.effectPool))
                    go = await _poolManager.GetObjectAsync(assetRef, _poolManager.effectPool);

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (!go)
                {
                    throw new System.Exception($"Failed to get barrel prefab for type {type} from pool.");
                }
            }
            catch (System.OperationCanceledException)
            {
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BarrelManager] GenerateBarrelAsync: {ex.Message}");
                return;
            }

            var pos = MapManager.Instance.GetRandomPointInMap();

            go.transform.position = pos;
            go.GetComponent<Barrel>().InitBarrel(type);
            go.SetActive(true);
        }



        public void UpdateBarrelSkill(EffectType type, int count)
        {
            if (_barrelConfigDict.TryGetValue(type, out var barrelConfig))
            {
                barrelConfig.attackCountPerBarrel = count;
            }
            else
            {
                _barrelConfigDict[type] = new BarrelConfig()
                {
                    type = type,
                    attackCountPerBarrel = count,
                };
            }
        }

        public void MeteorAttackActive(EffectType type)
        {
            var config = _barrelConfigDict[type];
            int count = config.attackCountPerBarrel;

            for (int i = 0; i < count; i++)
                _meteorTargetQueue.Enqueue(type);

            int enemyCount = _enemyManager.Enemies.Count;

            if (enemyCount <= 0)
                return;
            else
                MeteorAttack();
        }

        private void MeteorAttack()
        {
            while (_meteorTargetQueue.Count > 0)
            {
                var type = _meteorTargetQueue.Dequeue();
                int enemyCount = _enemyManager.Enemies.Count;

                if (enemyCount <= 0)
                    break;

                int randomIndex = Random.Range(0, enemyCount);
                var enemy = _enemyManager.Enemies[randomIndex];

                AttackAsync(type, enemy.transform.position).Forget();
            }
        }

        private async Awaitable AttackAsync(EffectType type, Vector3 pos)
        {
            GameObject go = null;

            var assetRef = _barrelSoDict[type].meteorPrefab;

            try
            {
                if (!_poolManager.TryGetObject(assetRef, out go, _poolManager.effectPool))
                    go = await _poolManager.GetObjectAsync(assetRef, _poolManager.effectPool);

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (!go)
                {
                    Debug.LogError("[BarrelManager] AttackAsync: failed to get meteor from pool.");
                    throw new System.Exception("Failed to get meteor prefab from pool.");
                }

                var meteor = go.GetComponent<Meteor>();
                float y = MapManager.Instance.GetMapPosition().y + meteorYOffset;

                var stat = PlayerController.Instance.Stat;
                float damage = stat.AttackPower * meteorDamageModifier;
                var damageInfo = new DamageInfo(damage, type, stat);

                var meteorConfig = new MeteorConfig(
                    position: Utils.GetXZPosition(pos) + new Vector3(0, y, 0),
                    damageInfo: damageInfo
                );

                meteor.InitMeteor(meteorConfig);

                go.SetActive(true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BarrelManager] AttackAsync: {ex.Message}");
                return;
            }
        }
    }
}