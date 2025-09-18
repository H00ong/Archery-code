using Game.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OrbConfig
{
    public List<Orb> orbs = new();
    public bool clockwise;
    public float distance = -1;
}

public class OrbManager : MonoBehaviour
{
    [SerializeField] Transform orbPivot;
    [SerializeField] private string label;
    [SerializeField] float defaultDistance;
    int generatedOrbSetCount = 0;
    bool clockwise = true;

    public float GeneratedOrbSetDistance =>
    defaultDistance + .5f * (generatedOrbSetCount - 1);

    Dictionary<OrbType, OrbScriptable> orbSoDict = new();
    Dictionary<OrbType, OrbConfig> orbObjDict = new();

    AsyncOperationHandle<IList<OrbScriptable>> _handle;

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
        if (orbPivot == null)
        {
            orbPivot = FindAnyObjectByType<OrbPivot>().transform;
        }
    }

    private void CreateOrbObjDict()
    {
        if (orbObjDict == null) orbObjDict = new();
        else orbObjDict.Clear();

        foreach (OrbType type in Enum.GetValues(typeof(OrbType)))
        {
            orbObjDict[type] = new OrbConfig();
        }
    }

    private IEnumerator EnsureOrbDictReady()
    {
        if (orbSoDict != null && orbSoDict.Count > 0)
            yield break;

        if (orbSoDict == null)
            orbSoDict = new();

        _handle = Addressables.LoadAssetsAsync<OrbScriptable>(
            label,
            so => orbSoDict[so.orbType] = so
        );

        yield return _handle;

        if (_handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load {label} failed");
        }
    }

    public void GenerateOrb(OrbType _type, int _count)
    {
        StartCoroutine(GenerateOrbCoroutine(_type, _count));
    }

    IEnumerator GenerateOrbCoroutine(OrbType _type, int _orbCount)
    {
        int _count = _orbCount;

        if (orbObjDict == null)
            CreateOrbObjDict();

        int beforeCount = orbObjDict[_type].orbs.Count;

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
            var orbSo = orbSoDict[_type];

            yield return PoolManager.Instance.GetObject(orbSo.orb, inst => go = inst);

            if (go == null)
            {
                Debug.LogError("Gameobject is null");
                yield break;
            }

            go.transform.SetParent(orbPivot);

            var _orb = go.GetComponent<Orb>();
            orbObjDict[_type].orbs.Add(_orb);
        }

        InitOrbType(_type);
    }

    private void InitOrbType(OrbType _type)
    {
        if (!orbObjDict.TryGetValue(_type, out var orbConfig) || orbConfig.orbs == null || orbConfig.orbs.Count == 0)
            return;
        
        if(orbConfig.distance <= 0)
        {
            orbConfig.distance = GeneratedOrbSetDistance;
            generatedOrbSetCount++;

            orbConfig.clockwise = clockwise;
            clockwise = !clockwise;
        }

        int count = orbConfig.orbs.Count;

        for (int i = 0; i < count; i++)
        {
            var obj = orbConfig.orbs[i];

            obj.InitilaizeOrb(orbPivot, orbConfig.clockwise);
            SetOrbPosition(obj.transform, orbConfig.distance, count, i);
            obj.gameObject.SetActive(true);
        }

    }

    private void SetOrbPosition(Transform _obj, float distance, int totalCount, int index)
    {
        Transform _pivot = orbPivot;

        // 기본 방향은 pivot의 forward
        _obj.position = _pivot.position + _pivot.forward * distance;

        // 피벗 기준으로 각도만큼 회전
        float angle = (360f / totalCount) * index;
        _obj.RotateAround(_pivot.position, Vector3.up, angle);
    }
}