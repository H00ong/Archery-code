using Enemy;
using UnityEngine;

namespace Players
{
    public class PlayerHurt : MonoBehaviour
    {
        [SerializeField] PlayerController playerManager;
        [SerializeField] Health playerHealth;

        public void Init()
        {
            if (playerHealth == null) playerHealth = GetComponent<Health>();

            // Health 이벤트 구독
            playerHealth.OnDie += OnPlayerDie;
            playerHealth.OnHit += OnPlayerHit;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDie -= OnPlayerDie;
                playerHealth.OnHit -= OnPlayerHit;
            }
        }

        private void OnPlayerDie()
        {
            playerManager.ChangePlayerState(PlayerState.Dead);
        }

        private void OnPlayerHit()
        {
            // 피격 시 효과 (애니메이션, 사운드 등)
        }

        public void TakeHeal(int healAmount, out bool valid) 
        {
            playerHealth.TakeHeal(healAmount, out valid);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (PlayerController.Instance.IsPlayerDead)
                return;

            var hitRoot = collision.collider.attachedRigidbody ? collision.collider.attachedRigidbody.gameObject
                : collision.collider.gameObject;

            if (!hitRoot.TryGetComponent<EnemyController>(out var enemy)) return;
            float atk = enemy.GetAtk();
            var damageInfo = new DamageInfo(atk, DamageType.Normal, hitRoot);
            playerHealth.TakeDamage(damageInfo);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!playerManager) playerManager = GetComponent<PlayerController>();
            if (!playerHealth) playerHealth = GetComponent<Health>();
        }
#endif
    }
}
