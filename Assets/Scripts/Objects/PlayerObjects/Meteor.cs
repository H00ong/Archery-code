using System.Collections;
using Enemies;
using Enemy;
using Managers;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] float _terminateTime = 2f;      // meteor ������� �ð�
    [SerializeField] float _timeDelayForMeteor = 1f; // ���� burst �ð� delay

    [SerializeField] LayerMask _enemyLayer;
    [SerializeField] float _attackRange = 1f;

    int   _atk;
    float _timer = 0f;

    private void OnEnable()
    {
        StartCoroutine(TerminateCoroutine());
    }

    public void Init(Vector3 pos, int atk)
    {
        transform.position = pos;
        transform.rotation = Quaternion.identity;

        _atk = atk;
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
                    enemy.TakeDamage(_atk);
            }
        }
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
