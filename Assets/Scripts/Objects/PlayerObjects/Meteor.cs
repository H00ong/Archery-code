using System.Collections;
using System.Collections.Generic;
using Enemy;
using Managers;
using Objects;
using Players;
using UnityEngine;

public readonly struct MeteorConfig
{
    public readonly Vector3 position;
    public readonly DamageInfo damageInfo;

    public MeteorConfig(Vector3 position, DamageInfo damageInfo)
    {
        this.position = position;
        this.damageInfo = damageInfo;
    }
}

public class Meteor : SceneObject
{
    [SerializeField] float terminateTime = 2f;
    [SerializeField] float timeDelayForMeteor = 1f;

    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackRange = 1f;

    DamageInfo _damageInfo;
    float _timer = 0f;
    
    private HashSet<IDamageable> _hitEnemies = new HashSet<IDamageable>();

    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(TerminateCoroutine());
        _hitEnemies.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void InitMeteor(MeteorConfig config)
    {
        transform.position = config.position;
        transform.rotation = Quaternion.identity;

        _damageInfo = config.damageInfo;
        _damageInfo.attackSource = gameObject;
        _timer = 0f;
    }

    private void Update()
    {
        if (_timer >= timeDelayForMeteor)
            AttackCheck();
        else
            _timer += Time.deltaTime;
    }

    private void AttackCheck()
    {
        var cds = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (cds.Length > 0)
        {
            foreach (var cd in cds)
            {
                var enemy = cd.GetComponentInParent<IDamageable>();
                
                if (enemy != null && _hitEnemies.Add(enemy))
                {
                    enemy.TakeDamage(_damageInfo);
                    Debug.Log($"[Meteor] Hit {cd.gameObject.name} | Type: {_damageInfo.type} | Damage: {_damageInfo.damageAmount}");
                }
            }
        }
    }

    IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(terminateTime);

        _timer = 0f;
        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
