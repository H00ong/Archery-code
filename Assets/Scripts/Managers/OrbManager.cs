using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Players;
using Stat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbConfig
{
    public List<Orb> orbs = new();
    public float rotateSpeed;
    public float distance = -1;
}

public class OrbManager : MonoBehaviour
{
    public static OrbManager instance;

    [SerializeField] Transform _orbPivot;
    [SerializeField] private string _label;
    [SerializeField] float _defaultDistance;
    [SerializeField] float _orbDamageModifier = 1f;
    [SerializeField] float _defaultRotateSpeed = 40f;
    
    int _generatedOrbSetCount = 0;
    float _rotateSpeed = 40f;

    Dictionary<EffectType, OrbScriptable> _orbSoDict = new();
    Dictionary<EffectType, OrbConfig> _orbObjDict = new();

    AsyncOperationHandle<IList<OrbScriptable>> _handle;

    PoolManager _poolManager;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        _rotateSpeed = _defaultRotateSpeed;

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

        foreach (EffectType type in Enum.GetValues(typeof(EffectType)))
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
            so => _orbSoDict[so.effectType] = so
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
            foreach (var orb in orbConfig.orbs)
            {
                if (orb != null)
                    _poolManager.ReturnObject(orb.gameObject);
            }

            orbConfig.orbs.Clear();
            orbConfig.distance = -1;
        }

        _orbSoDict.Clear();

    }

    public void GenerateOrb(EffectType _type, int _count)
    {
        StartCoroutine(GenerateOrbCoroutine(_type, _count));
    }

    IEnumerator GenerateOrbCoroutine(EffectType _type, int _orbCount)
    {
        int _count = _orbCount;

        if (_orbObjDict == null)
        { 
            CreateOrbObjDict();
        }

        int beforeCount = _orbObjDict[_type].orbs.Count;

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
            var orbSo = _orbSoDict[_type];

            if (!PoolManager.Instance.TryGetObject(orbSo.orb, out var go))
            {
                yield return PoolManager.Instance.GetObject(orbSo.orb, obj => go = obj);
            }

            if (go == null)
            {
                Debug.LogError("Gameobject is null");
                yield break;
            }

            go.transform.SetParent(_orbPivot);

            var _orb = go.GetComponent<Orb>();
            _orbObjDict[_type].orbs.Add(_orb);
        }

        InitOrbType(_type);
    }

    private void InitOrbType(EffectType _type)
    {
        if (!_orbObjDict.TryGetValue(_type, out var orbConfig) || orbConfig.orbs == null || orbConfig.orbs.Count == 0)
            return;
        
        if(orbConfig.distance <= 0)
        {
            _generatedOrbSetCount++;
            orbConfig.distance = _defaultDistance + .5f * (_generatedOrbSetCount - 1);
            orbConfig.rotateSpeed = _rotateSpeed;
            _rotateSpeed = -_rotateSpeed;
        }

        int count = orbConfig.orbs.Count;

        for (int i = 0; i < count; i++)
        {
            var orb = orbConfig.orbs[i];

            var config = new OrbInitConfig(_orbPivot, orbConfig.rotateSpeed, _type, _orbDamageModifier);
            orb.InitializeOrb(config);
            SetOrbPosition(orb.transform, orbConfig.distance, count, i);
            orb.gameObject.SetActive(true);
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