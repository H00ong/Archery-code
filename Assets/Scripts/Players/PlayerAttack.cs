using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static float playerAttackSpeed;
    [SerializeField] float defaultAttackSpeed;

    private void Start()
    {
        SetPlayerAttackSpeed();
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
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
        }
    }

    public void SetPlayerAttackSpeed() 
    {
        // defaultAttackSpeed = GameManager.Instance.DataManager.GetPlayerData().attackSpeed;
        playerAttackSpeed = defaultAttackSpeed;
    }
}
