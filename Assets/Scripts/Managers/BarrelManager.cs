using System.Collections;
using System.Collections.Generic;
using Map;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public sealed class BarrelConfig
    {
        public BarrelType Type;
        public int AttackCountPerBarrel = 0;
        public float Timer = 0f;
    }

    public class BarrelManager : MonoBehaviour
    {
        [SerializeField] private string addressableAssetLabel;
        [SerializeField] private float barrelGenerateTime = 5f;
        [SerializeField] private int atk = 1;
        [SerializeField] private float yOffset = 1f;

        private Dictionary<BarrelType, BarrelScriptable> _barrelSoDict = new();
        private readonly Dictionary<BarrelType, BarrelConfig> _barrelConfigDict = new();
        private Queue<BarrelType> _attackQueue = new();

        private AsyncOperationHandle<IList<BarrelScriptable>> _handle; 
        private Bounds _currentMapBound;

        private PoolManager _poolManager;
        private EnemyManager _enemyManager;

        private void Start()
        {
            _poolManager = PoolManager.Instance;
            _enemyManager = EnemyManager.Instance;
        }
    
        private void OnEnable()
        {
            StartCoroutine(EnsureBarrelDictReady());

            EventBus.Subscribe(EventType.StageCombatStarted, MeteorAttack);
            EventBus.Subscribe(EventType.StageCombatStarted, GetBoundsOfMap);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.StageCombatStarted, MeteorAttack);
            EventBus.Unsubscribe(EventType.StageCombatStarted, GetBoundsOfMap);
        }

        private void Update()
        {
            GenerateBarrelCheck();
        }

        private void GetBoundsOfMap() 
        {
            var go = MapManager.Instance.currentMap;
            
            if (!go) 
            {
                Debug.LogError("CurrentMap is null");
            }

            var map = go.GetComponent<GameMap>();
            _currentMapBound = map.GetBounds();
        }

        private void GenerateBarrelCheck()
        {
            if (!StageManager.Instance.IsInCombat) return;
        
            foreach (var key in _barrelConfigDict.Keys)
            {
                var config = _barrelConfigDict[key];
                config.Timer += Time.deltaTime;
            
                if (!(config.Timer >= barrelGenerateTime)) continue;
            
                StartCoroutine(GenerateBarrel(config.Type));
                config.Timer = 0f;
            }
        }

        private IEnumerator GenerateBarrel(BarrelType type)
        {
            var assetRef = _barrelSoDict[type].barrelPrefab;

            if(!_poolManager.TryGetObject(assetRef, out var go, _poolManager.EffectPool))
                yield return _poolManager.GetObject(assetRef, inst => go = inst, _poolManager.EffectPool);
        
            bool isInside = false;
            Vector3 pos = Vector3.zero;

            while (!isInside)
            {
                pos = RandomPointInBounds(_currentMapBound);

                if (NavMesh.SamplePosition(pos, out var hit, 1f, NavMesh.AllAreas))
                {
                    isInside = true;
                    pos = hit.position;
                }
            }

            go.transform.position = pos;
            go.GetComponent<Barrel>().Init(type);
            go.SetActive(true);
        }

        Vector3 RandomPointInBounds(Bounds b)
        {
            return new Vector3(
                UnityEngine.Random.Range(b.min.x, b.max.x),
                UnityEngine.Random.Range(b.min.y, b.max.y),
                UnityEngine.Random.Range(b.min.z, b.max.z)
            );
        }

        private IEnumerator EnsureBarrelDictReady()
        {
            if (_barrelSoDict is { Count: > 0 })
                yield break;

            _barrelSoDict ??= new Dictionary<BarrelType, BarrelScriptable>();

            _handle = Addressables.LoadAssetsAsync<BarrelScriptable>(
                addressableAssetLabel,
                so => _barrelSoDict[so.type] = so
            );

            yield return _handle;

            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Load {addressableAssetLabel} failed");
            }
        }

        public void UpdateBarrelSkill(BarrelType type, int count)
        {
            if (_barrelConfigDict.TryGetValue(type, out var barrelConfig))
            {
                barrelConfig.AttackCountPerBarrel = count;
            }
            else
            {
                _barrelConfigDict[type] = new BarrelConfig()
                {
                    Type = type,
                    AttackCountPerBarrel = count,
                };
            }
        }

        public void BarrelAttackActive(BarrelType type)
        {
            var config = _barrelConfigDict[type];
            int count = config.AttackCountPerBarrel;

            for (int i = 0; i < count; i++)
                _attackQueue.Enqueue(type);

            int enemyCount = _enemyManager.Enemies.Count;

            if (enemyCount <= 0) return;
            else MeteorAttack();
        }

        private void MeteorAttack() 
        {
            _attackQueue ??= new Queue<BarrelType>();

            if (_attackQueue.Count <= 0) return;

            while (_attackQueue.Count > 0) 
            {
                var type = _attackQueue.Dequeue();
                int enemyCount = _enemyManager.Enemies.Count;

                if (enemyCount <= 0) break;

                int randomIndex = UnityEngine.Random.Range(0, enemyCount);
                var enemy = _enemyManager.Enemies[randomIndex];

                StartCoroutine(AttackCoroutine(type, enemy.transform.position));
            }
        }

        private IEnumerator AttackCoroutine(BarrelType type, Vector3 pos)
        {
            GameObject go = null;

            if(!_poolManager.TryGetObject(_barrelSoDict[type].meteorPrefab, out go, _poolManager.EffectPool))
                yield return PoolManager.Instance.GetObject(_barrelSoDict[type].meteorPrefab, inst => go = inst, _poolManager.EffectPool);

            var meteor = go.GetComponent<Meteor>();
            float y = MapManager.Instance.currentMap.transform.position.y + yOffset;

            var effectType = Utils.BarrelTypeToEffectType(type);
            var damageInfo = new DamageInfo(atk, effectType);

            var meteorConfig = new MeteorInitConfig(
                position: Utils.GetXZPosition(pos) + new Vector3(0, y, 0),
                damageInfo: damageInfo
            );
            meteor.Initialize(meteorConfig);

            go.SetActive(true);
        }
    }
}