using System.Collections.Generic;
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
        private readonly HashSet<EnemyController> contactEnemies = new();
        private readonly HashSet<Collider> ignoredEnemyColliders = new();

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
            playerHealth.OnStatusChanged += OnPlayerStatusChanged;
        }

        private void FixedUpdate()
        {
            if (playerHealth == null || playerHealth.IsDead() || !playerCollider.enabled)
                return;

            var overlappingEnemies = new HashSet<EnemyController>();
            Collider[] colliders = Physics.OverlapBox(
                playerCollider.bounds.center,
                playerCollider.bounds.extents,
                Quaternion.identity);

            foreach (var collider in colliders)
            {
                GameObject hitRoot = collider.attachedRigidbody
                    ? collider.attachedRigidbody.gameObject
                    : collider.gameObject;

                if (!hitRoot.TryGetComponent<EnemyController>(out var enemy))
                    continue;

                IgnoreEnemyPush(enemy);
                overlappingEnemies.Add(enemy);

                if (contactEnemies.Add(enemy))
                    TakeEnemyContactDamage(enemy, hitRoot);
            }

            contactEnemies.RemoveWhere(enemy => !overlappingEnemies.Contains(enemy));
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDie -= OnPlayerDie;
                playerHealth.OnHit -= OnPlayerHit;
                playerHealth.OnStatusChanged -= OnPlayerStatusChanged;
            }

            contactEnemies.Clear();
        }

        private void OnPlayerDie()
        {
            playerController.ChangePlayerAnimation(PlayerState.Dead);

            playerCollider.enabled = false;
            playerRigidbody.isKinematic = true;
            contactEnemies.Clear();
        }

        private void OnPlayerHit()
        {
            Debug.Log("Player Hit!");
        }

        private void OnPlayerStatusChanged(DamageInfo damageInfo, bool isStart)
        {
            if (!playerController.enableIceSlowEffect) return;
            if (!Utils.HasEffectType(damageInfo.type, EffectType.Ice)) return;

            var iceData = damageInfo.GetEffectData(EffectType.Ice);
            if (iceData == null) return;

            if (isStart)
                playerController.Movement.UpdateMoveSpeed(-iceData.value);
            else
                playerController.Movement.UpdateMoveSpeed(0f);
        }

        public void Die()
        {
            EventBus.Publish(EventType.PlayerDied);
        }

        public bool TryTakeHeal(int healAmount)
        {
            return playerHealth.TryTakeHeal(healAmount);
        }

        private void IgnoreEnemyPush(EnemyController enemy)
        {
            foreach (var enemyCollider in enemy.enemyColliders)
            {
                if (!enemyCollider || ignoredEnemyColliders.Contains(enemyCollider))
                    continue;

                Physics.IgnoreCollision(playerCollider, enemyCollider, true);
                ignoredEnemyColliders.Add(enemyCollider);
            }
        }

        private void TakeEnemyContactDamage(EnemyController enemy, GameObject hitRoot)
        {
            if (PlayerController.Instance.IsPlayerDead || enemy.health == null)
                return;
                
            float atk = enemy.GetAtk();
            var damageInfo = new DamageInfo(atk, enemy.stat.AttackEffectType, enemy.stat, hitRoot);
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
    
