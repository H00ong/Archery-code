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
            portalCd.enabled = true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.StageCleared, ActivePortal);
        
            DeactivePortal();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.StageCleared, ActivePortal);
        }


        private void OnTriggerEnter(Collider other)    
        {
            if (other.CompareTag(playerTag)) 
            {
                StageManager.Instance.HandleCommand(StageCommandType.EnterPortal);
            }
        }
    }
}
