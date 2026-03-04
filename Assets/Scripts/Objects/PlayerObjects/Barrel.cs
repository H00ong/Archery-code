using Game.Player;
using System.Collections;
using Managers;
using UnityEngine;
using Players;

public class Barrel : MonoBehaviour
{
    [SerializeField] float _lifeTime = 20f;
    [SerializeField] EffectType _type;
    private BarrelManager _barrelManager;

    private void OnEnable()
    {
        StartCoroutine(TerminateCoroutine());
    }

    private void Start()
    {
        _barrelManager = BarrelManager.Instance;
    }

    public void InitBarrel(EffectType type)
    {
        _type = type;
    }

    IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(_lifeTime);

        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Utils.TagMap[TagType.Player]))
        {
            BarrelManager.Instance.MeteorAttackActive(_type);

            StopAllCoroutines();
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
