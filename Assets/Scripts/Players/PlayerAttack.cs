using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Player Attack Info")]
    [SerializeField] GameObject magicBeam;
    public static float playerAttackSpeed;
    [SerializeField] float defaultAttackSpeed;
    [SerializeField] float defaltProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    public int playerDamage = 50; // Default player damage, can be modified later

    [Header("Player Projectile Info")]
    public static float projectileSpeed;
    [SerializeField] float defaultProjectileSpeed;

    public static Enemy CurrentTarget;

    private void Start()
    {
        DefaultSetting();
    }

    public void Attack()
    {
        Enemy target = GetEenemyTarget();

        if (target != null)
        {
            CurrentTarget = target;
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
    }

    private Enemy GetEenemyTarget()
    {
        Enemy target = null;
        float minDistance = float.MaxValue;

        if (EnemyManager.enemies.Count <= 0)
            return null;

        foreach (Enemy enemy in EnemyManager.enemies)
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
        GameObject go = GameManager.Instance.StageManager.PoolManager.GetObject(magicBeam);

        Vector3 projectileDir = Utils.GetDirectionVector(CurrentTarget.transform.position, _shootingPoint);

        Projectile_Player newProjectile = go.GetComponent<Projectile_Player>();
        newProjectile.SetupProjectile(_shootingPoint, projectileDir, projectileSpeed, playerDamage, defaltProjectileLifetime);
    }

    void DefaultSetting()
    {
        playerAttackSpeed = defaultAttackSpeed;
        projectileSpeed = defaultProjectileSpeed;
    }
}
