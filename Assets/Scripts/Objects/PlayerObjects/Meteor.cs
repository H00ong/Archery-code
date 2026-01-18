using System.Collections;
using Enemy;
using Managers;
using Players;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] float _terminateTime = 2f;
    [SerializeField] float _timeDelayForMeteor = 1f;

    [SerializeField] LayerMask _enemyLayer;
    [SerializeField] float _attackRange = 1f;

    int _atk;
    BarrelType _barrelType;
    float _timer = 0f;

    private void OnEnable()
    {
        StartCoroutine(TerminateCoroutine());
    }

    public void Init(Vector3 pos, int atk, BarrelType barrelType = BarrelType.Common)
    {
        transform.position = pos;
        transform.rotation = Quaternion.identity;

        _atk = atk;
        _barrelType = barrelType;
        _timer = 0f;
    }

    private void Update()
    {
        if (_timer >= _timeDelayForMeteor)
            AttackCheck();
        else
            _timer += Time.deltaTime;
    }

    private void AttackCheck()
    {
        var cds = Physics.OverlapSphere(transform.position, _attackRange, _enemyLayer);

        if (cds.Length > 0)
        {
            foreach (var cd in cds)
            {
                if (cd.TryGetComponent<IDamageable>(out var enemy))
                {
                    DamageType damageType = ConvertBarrelTypeToDamageType(_barrelType);
                    var damageInfo = new DamageInfo(_atk, damageType, gameObject);
                    damageInfo.hitPoint = cd.ClosestPoint(transform.position);
                    enemy.TakeDamage(damageInfo);
                }
            }
        }
    }

    private DamageType ConvertBarrelTypeToDamageType(BarrelType type)
    {
        return type switch
        {
            BarrelType.Venom => DamageType.Venom,
            BarrelType.Blaze => DamageType.Fire,
            BarrelType.Ice => DamageType.Ice,
            _ => DamageType.Normal
        };
    }

    IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(_terminateTime);

        _timer = 0f;
        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
