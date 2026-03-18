using Managers;
using Players;
using UnityEngine;

namespace Objects
{
    public abstract class CollectItem : SceneObject
    {
        [SerializeField] protected float magnetRange = 3f;
        [SerializeField] protected float magnetSpeed = 30f;

        private static int _activeCount;
        public static int ActiveCount => _activeCount;
        private bool _isMovingToPlayer;

        protected Transform playerTransform;
        private bool _collected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _activeCount = 0;
        }

        private void Start()
        {
            if (PlayerController.Instance != null)
                playerTransform = PlayerController.Instance.transform;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _activeCount++;
            _collected = false;
            _isMovingToPlayer = false;
        }

        protected override void OnDisable()
        {
            if (!_collected)
                _activeCount--;

            base.OnDisable();
        }

        private void Update()
        {
            if (playerTransform == null) return;

            if (_isMovingToPlayer || StageManager.Instance.WaitingForCollectibles)
            {
                transform.position = Vector3.MoveTowards(
                        transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
            else if (Vector3.Distance(transform.position, playerTransform.position) <= magnetRange)
            {
                _isMovingToPlayer = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                Collect();
        }

        private void Collect()
        {
            if (_collected) return;
            _collected = true;
            _activeCount--;

            OnCollected();
            PoolManager.Instance.ReturnObject(gameObject);

            if (_activeCount <= 0)
                EventBus.Publish(EventType.AllCollectiblesCollected);
        }

        protected abstract void OnCollected();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
