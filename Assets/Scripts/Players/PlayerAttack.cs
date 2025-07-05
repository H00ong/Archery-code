using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Player Attack Speed Info")]
    public static float playerAttackSpeed;
    [SerializeField] float defaultAttackSpeed;

    [Header("Player Projectile Speed Info")]
    public static float projectileSpeed;
    [SerializeField] float defaultProjectileSpeed;

    public static Enemy CurrentTarget;

    private void Start()
    {
        SetPlayerDefaultAttackSpeed();
        SetDefaultProjectileSpeed();
    }

    public void Attack() 
    {
        Enemy target = null;
        float minDistance = float.MaxValue;

        foreach (Enemy enemy in EnemyManager.enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = enemy;
            }
        }

        if (target != null) 
        {
            CurrentTarget = target;
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
        }
    }

    public void SetPlayerDefaultAttackSpeed() 
    {
        // defaultAttackSpeed = GameManager.Instance.DataManager.GetPlayerData().attackSpeed;
        playerAttackSpeed = defaultAttackSpeed;
    }

    public void SetDefaultProjectileSpeed() 
    {
        projectileSpeed = defaultProjectileSpeed;
    }
}
