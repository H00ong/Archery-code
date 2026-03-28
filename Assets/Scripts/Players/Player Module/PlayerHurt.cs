using Enemy;
using UnityEngine;

namespace Players
{
    public class PlayerHurt : MonoBehaviour
    {
        [SerializeField] private Collider playerCollider;
        [SerializeField] private Rigidbody playerRigidbody;

        private PlayerController playerController;
        private Health playerHealth;

        void Awake()
        {
            CachingComponent();
        }

        private void CachingComponent()
        {
            if (playerCollider == null)
                playerCollider = GetComponent<Collider>();

            if (playerRigidbody == null)
                playerRigidbody = GetComponent<Rigidbody>();
        }

        public void Init()
        {
            playerController = PlayerController.Instance;
            playerHealth = playerController.Health;

            playerCollider.enabled = true;
            playerRigidbody.isKinematic = false;

            playerHealth.OnDie += OnPlayerDie;
            playerHealth.OnHit += OnPlayerHit;
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDie -= OnPlayerDie;
                playerHealth.OnHit -= OnPlayerHit;
            }
        }

        private void OnPlayerDie()
        {
            playerController.ChangePlayerAnimation(PlayerState.Dead);

            playerCollider.enabled = false;
            playerRigidbody.isKinematic = true;
        }

        private void OnPlayerHit()
        {
            Debug.Log("Player Hit!");
        }

        public void Die()
        {
            EventBus.Publish(EventType.PlayerDied);
        }

        public bool TryTakeHeal(int healAmount)
        {
            return playerHealth.TryTakeHeal(healAmount);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (PlayerController.Instance.IsPlayerDead)
                return;

            var hitRoot = collision.collider.attachedRigidbody ? collision.collider.attachedRigidbody.gameObject
                : collision.collider.gameObject;

            if (!hitRoot.TryGetComponent<EnemyController>(out var enemy))
                return;
                
            float atk = enemy.GetAtk();
            var damageInfo = new DamageInfo(atk, EffectType.Normal, hitRoot);
            playerHealth.TakeDamage(damageInfo);
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            CachingComponent();
        }
#endif
    }
}
    
