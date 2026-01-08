using Enemies;
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
        }

        public void TakeDamage(float damage)
        {
            playerHealth.TakeDamage(damage);

            if (playerHealth.IsDead()) 
            {
                playerManager.ChangePlayerState(PlayerState.Dead);
            }
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

            TakeDamage(atk);
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
