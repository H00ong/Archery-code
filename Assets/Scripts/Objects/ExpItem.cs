using UnityEngine;
using Managers;
using Players;

namespace Objects
{
    public class ExpItem : MonoBehaviour
    {
        [SerializeField] private int expAmount = 10;
        [SerializeField] private float magnetRange = 3f;
        [SerializeField] private float magnetSpeed = 5f;

        private Transform _playerTransform;
        private bool _isMovingToPlayer;

        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                _playerTransform = PlayerController.Instance.transform;
            }
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (distance < magnetRange)
            {
                _isMovingToPlayer = true;
            }

            if (_isMovingToPlayer)
            {
                transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, magnetSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LevelManager.Instance.AddExp(expAmount);
                PoolManager.Instance.ReturnObject(gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
