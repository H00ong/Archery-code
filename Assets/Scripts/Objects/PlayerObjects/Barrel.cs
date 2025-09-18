using Game.Player;
using System.Collections;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] float _lifeTime = 20f;
    [SerializeField] BarrelType _type;
    private BarrelManager _barrelManager;

    private void OnEnable()
    {
        StartCoroutine(TerminateCoroutine());
    }

    public void Init(BarrelType type) 
    {
        _type = type;

        _barrelManager = FindAnyObjectByType<BarrelManager>();
    }

    IEnumerator TerminateCoroutine() 
    {
        yield return new WaitForSeconds(_lifeTime);

        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = Utils.GetCollisionRoot(other);

        if (go.GetComponent<PlayerManager>() != null)
        {
            _barrelManager.BarrelAttackActive(_type);

            StopAllCoroutines();
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
