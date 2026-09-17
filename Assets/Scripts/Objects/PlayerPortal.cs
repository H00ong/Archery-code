using Game.Stage.Management;
using UnityEngine;

namespace Objects
{
    public class PlayerPortal : MonoBehaviour
    {
        private readonly string playerTag = Utils.ToString(TagType.Player);
        
        [SerializeField] private Collider obstacleCd;
        [SerializeField] private Collider portalCd;
        
        private void DeactivePortal()
        {
            obstacleCd.enabled = true;
            portalCd.enabled = false;
        }

        private void ActivePortal()
        {
            obstacleCd.enabled = false;
            portalCd.enabled = true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.AllCollectiblesCollected, ActivePortal);
        
            DeactivePortal();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.AllCollectiblesCollected, ActivePortal);
        }


        private void OnTriggerEnter(Collider other)
        {
            OnPlayerEnterPortal(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnPlayerEnterPortal(other);
        }

        private void OnPlayerEnterPortal(Collider other)
        {
            GameObject root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            if (root.CompareTag(playerTag))
            {
                StageManager.Instance.HandleCommand(StageCommandType.EnterPortal);
            }
        }
    }
}
