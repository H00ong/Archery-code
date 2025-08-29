using UnityEngine;

public class PlayerHurt : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;
    PlayerHeatlh playerHealth;
    string enemyTag = "Enemy";

    private void Start()
    {
        playerHealth = GetComponent<PlayerHeatlh>();

        playerHealth.InitializeHealth();
    }

    public void GetHit(float _damage)
    {
        if (PlayerManager.IsPlayerDead)
            return;

        // 데미지 감소 로직

        playerHealth.TakeDamage(_damage);

        if (playerHealth.IsDead()) 
        {
            playerManager.ChangePlayerState(PlayerState.Dead);
        }
        else
        {
            // 피격 효과
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PlayerManager.IsPlayerDead)
            return;

        // 충돌한 오브젝트가 적의 공격인지 확인

        if (Utils.CompareTag(collision, enemyTag)) 
        {
            float damage = 0f;

            EnemyController enemy = collision.gameObject.GetComponentInParent<EnemyController>();
            damage = enemy.GetAtk();

            GetHit(damage);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerManager == null) playerManager = FindAnyObjectByType<PlayerManager>();
    }
#endif
}
