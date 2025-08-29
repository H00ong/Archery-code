using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerAttack : MonoBehaviour
{
    [Header("Required Mangagers")]
    [SerializeField] PoolManager poolManager;
    [Space]
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject playerProjectile;
    [SerializeField] Transform projectileParent;
    [Space]
    [Header("Player Attack Info")]
    public static EnemyController CurrentTarget;
    public static float playerAttackSpeed;
    public int playerDamage = 50; // Default player damage, can be modified later
    [Space]
    public static float projectileSpeed;
    [SerializeField] float defaultAttackSpeed; // animation speed
    [SerializeField] float defaultProjectileSpeed;
    [SerializeField] float defaltProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used


    private void Start()
    {
        DefaultSetting();
    }

    public void Attack()
    {
        EnemyController target = GetEenemyTarget();

        if (target != null)
        {
            CurrentTarget = target;
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
    }

    private EnemyController GetEenemyTarget()
    {
        EnemyController target = null;
        float minDistance = float.MaxValue;

        if (EnemyManager.enemies.Count <= 0)
            return null;

        foreach (EnemyController enemy in EnemyManager.enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = enemy;
            }
        }

        return target;
    }

    public void Shoot(Vector3 _shootingPoint)
    {
        StartCoroutine(ShootingCoroutine(_shootingPoint));
    }

    IEnumerator ShootingCoroutine(Vector3 _shootingPoint) 
    {
        GameObject go = null;

        yield return poolManager.GetObject(playerProjectile, inst => go = inst, projectileParent);

        if (go == null) yield break;

        Projectile_Player newProjectile = go.GetComponent<Projectile_Player>();

        if (newProjectile == null) yield break;

        Vector3 projectileDir = Utils.GetDirectionVector(CurrentTarget.transform.position, _shootingPoint);
        newProjectile.SetupProjectile(_shootingPoint, projectileDir, projectileSpeed, playerDamage, defaltProjectileLifetime);
    }

    void DefaultSetting()
    {
        playerAttackSpeed = defaultAttackSpeed;
        projectileSpeed = defaultProjectileSpeed;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (poolManager == null) poolManager = FindAnyObjectByType<PoolManager>();
    }
#endif
}
