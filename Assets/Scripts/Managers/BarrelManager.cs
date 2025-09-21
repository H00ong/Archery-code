using Game.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class BarrelConfig
{
    public BarrelType Type;
    public int AttackCountPerBarrel = 0;
    public float Timer = 0f; // barrel 생성 시간
}

public class BarrelManager : MonoBehaviour
{
    [SerializeField] string _label;
    [SerializeField] float _barrelGenerateTime = 5f;
    [SerializeField] int _atk = 1;

    Dictionary<BarrelType, BarrelScriptable> _barrelSoDict = new();
    readonly Dictionary<BarrelType, BarrelConfig> _barrelConfigDict = new();
    Queue<BarrelType> _attackQueue = new();

    AsyncOperationHandle<IList<BarrelScriptable>> _handle;
    Bounds _currentMapBound;
    
    public void Init()
    {
        StartCoroutine(EnsureBarrelDictReady());

        EnemyManager.OnAllEnemiesSpawned += MeteorAttack;

        MapManager.OnMapUpdated += () =>
        {
            var go = MapManager.CurrentMap;
            if (go == null) return;

            _currentMapBound = go.GetComponent<Map>().GetBounds();
        };
    }

    private void OnEnable()
    {
        Init();
    }

    private void Update()
    {
        GenerateBarrelCheck();
    }

    private void GenerateBarrelCheck()
    {
        if (StageManager.IsInCombat) 
        {
            foreach (var key in _barrelConfigDict.Keys)
            {
                var config = _barrelConfigDict[key];
                config.Timer += Time.deltaTime;
                if (config.Timer >= _barrelGenerateTime)
                {
                    StartCoroutine(GenerateBarrel(config.Type));
                    config.Timer = 0f;
                }
            }
        }
    }

    private IEnumerator GenerateBarrel(BarrelType type) 
    {
        var assetRef = _barrelSoDict[type].barrelPrefab;

        yield return PoolManager.Instance.GetObject(
            assetRef,
            (inst) =>
            {
                GameObject go = inst;
                bool isInside = false;
                Vector3 pos = Vector3.zero;

                while (isInside) 
                {
                    RandomPointInBounds(_currentMapBound);

                    if (NavMesh.SamplePosition(pos, out var hit, 1f, NavMesh.AllAreas))
                    { 
                        isInside = true;
                        pos = hit.position;
                    }
                }

                go.transform.position = pos;
                go.GetComponent<Barrel>().Init(type);
                inst.SetActive(true);
            },
            PoolManager.Instance.EffectPool
        );
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
        if (_barrelSoDict != null && _barrelSoDict.Count > 0)
            yield break;

        _barrelSoDict ??= new();

        _handle = Addressables.LoadAssetsAsync<BarrelScriptable>(
            _label,
            so => _barrelSoDict[so.type] = so
        );

        yield return _handle;

        if (_handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load {_label} failed");
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
        var _config = _barrelConfigDict[type];
        int count = _config.AttackCountPerBarrel;

        for (int i = 0; i < count; i++)
            _attackQueue.Enqueue(type);

        int enemyCount = EnemyManager.Enemies.Count;

        if (enemyCount <= 0) return;
        else MeteorAttack();
    }

    public void MeteorAttack() 
    {

        _attackQueue ??= new Queue<BarrelType>();

        if (_attackQueue.Count <= 0) return;

        while (_attackQueue.Count > 0) 
        {
            var type = _attackQueue.Dequeue();
            int enemyCount = EnemyManager.Enemies.Count;

            if (enemyCount <= 0) break;

            int randomIndex = UnityEngine.Random.Range(0, enemyCount);
            var enemy = EnemyManager.Enemies[randomIndex];

            StartCoroutine(AttackCoroutine(type, enemy.transform.position));
        }
    }

    IEnumerator AttackCoroutine(BarrelType type, Vector3 pos)
    {
        GameObject go = null;

        yield return PoolManager.Instance.GetObject(
            _barrelSoDict[type].meteorPrefab,
            (inst) =>
            {
                go = inst;

                var meteor = go.GetComponent<Meteor>();
                meteor.Init
                (
                    pos: Utils.GetXZPosition(pos),
                    atk: _atk
                );

                go.SetActive(true);
            },
            PoolManager.Instance.EffectPool
        );
    }
}