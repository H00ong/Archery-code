using Game.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbConfig
{
    public List<Orb> Orbs = new();
    public bool Clockwise;
    public float Distance = -1;
}

public class OrbManager : MonoBehaviour
{
    [SerializeField] Transform _orbPivot;
    [SerializeField] private string _label;
    [SerializeField] float _defaultDistance;
    [SerializeField] private int _orbDamage = 1;
    
    int _generatedOrbSetCount = 0;
    bool _clockwise = true;

    public float GeneratedOrbSetDistance =>
    _defaultDistance + .5f * (_generatedOrbSetCount - 1);

    Dictionary<OrbType, OrbScriptable> _orbSoDict = new();
    Dictionary<OrbType, OrbConfig> _orbObjDict = new();

    AsyncOperationHandle<IList<OrbScriptable>> _handle;

    PoolManager _poolManager;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        Bind();
        StartCoroutine(EnsureOrbDictReady());
        CreateOrbObjDict();
    }

    private void Bind()
    {
        if (_orbPivot == null)
        {
            _orbPivot = FindAnyObjectByType<OrbPivot>().transform;
        }

        _poolManager = PoolManager.Instance;
    }

    private void CreateOrbObjDict()
    {
        _orbSoDict.Clear();

        foreach (OrbType type in Enum.GetValues(typeof(OrbType)))
        {
            _orbObjDict[type] = new OrbConfig();
        }
    }

    private IEnumerator EnsureOrbDictReady()
    {
        if (_orbSoDict != null && _orbSoDict.Count > 0)
            yield break;

        if (_orbSoDict == null)
            _orbSoDict = new();

        _handle = Addressables.LoadAssetsAsync<OrbScriptable>(
            _label,
            so => _orbSoDict[so.orbType] = so
        );

        yield return _handle;

        if (_handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load {_label} failed");
        }
    }

    private void ClearOrbData() 
    {
        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
            _handle = default;
        }

        foreach (var kv in _orbObjDict)
        {
            var orbConfig = kv.Value;
            foreach (var orb in orbConfig.Orbs)
            {
                if (orb != null)
                    _poolManager.ReturnObject(orb.gameObject);
            }

            orbConfig.Orbs.Clear();
            orbConfig.Distance = -1;
        }

        _orbSoDict.Clear();

    }

    public void GenerateOrb(OrbType _type, int _count)
    {
        StartCoroutine(GenerateOrbCoroutine(_type, _count));
    }

    IEnumerator GenerateOrbCoroutine(OrbType _type, int _orbCount)
    {
        int _count = _orbCount;

        if (_orbObjDict == null)
            CreateOrbObjDict();

        int beforeCount = _orbObjDict[_type].Orbs.Count;

        if (beforeCount > 0 && _orbCount > beforeCount)
        {
            _count = _orbCount - beforeCount;
        }
        else if (beforeCount > _orbCount)
        {
            yield break;
        }

        for (int i = 0; i < _count; i++)
        {
            GameObject go = null;
            var orbSo = _orbSoDict[_type];

            yield return PoolManager.Instance.GetObject(orbSo.orb, inst => go = inst);

            if (go == null)
            {
                Debug.LogError("Gameobject is null");
                yield break;
            }

            go.transform.SetParent(_orbPivot);

            var _orb = go.GetComponent<Orb>();
            _orbObjDict[_type].Orbs.Add(_orb);
        }

        InitOrbType(_type);
    }

    private void InitOrbType(OrbType _type)
    {
        if (!_orbObjDict.TryGetValue(_type, out var orbConfig) || orbConfig.Orbs == null || orbConfig.Orbs.Count == 0)
            return;
        
        if(orbConfig.Distance <= 0)
        {
            orbConfig.Distance = GeneratedOrbSetDistance;
            _generatedOrbSetCount++;

            orbConfig.Clockwise = _clockwise;
            _clockwise = !_clockwise;
        }

        int count = orbConfig.Orbs.Count;

        for (int i = 0; i < count; i++)
        {
            var obj = orbConfig.Orbs[i];

            var config = new OrbInitConfig(_orbPivot, orbConfig.Clockwise, _type, _orbDamage);
            obj.Initialize(config);
            SetOrbPosition(obj.transform, orbConfig.Distance, count, i);
            obj.gameObject.SetActive(true);
        }

    }

    private void SetOrbPosition(Transform _obj, float distance, int totalCount, int index)
    {
        Transform _pivot = _orbPivot;

        _obj.position = _pivot.position + _pivot.forward * distance;

        float angle = (360f / totalCount) * index;
        _obj.RotateAround(_pivot.position, Vector3.up, angle);
    }
}