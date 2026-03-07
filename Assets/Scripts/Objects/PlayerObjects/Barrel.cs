using System.Collections;
using Managers;
using UnityEngine;
using Objects;

public class Barrel : SceneObject
{
    [SerializeField] float _lifeTime = 20f;
    [SerializeField] EffectType _type;
    private BarrelManager _barrelManager;
    private bool _isTouched = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(TerminateCoroutine());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Start()
    {
        _barrelManager = BarrelManager.Instance;
    }

    public void InitBarrel(EffectType type)
    {
        _type = type;
        _isTouched = false;
    }

    IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(_lifeTime);

        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Utils.TagMap[TagType.Player]) && !_isTouched)
        {
            _isTouched = true;

            BarrelManager.Instance.MeteorAttackActive(_type);

            StopAllCoroutines();
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
