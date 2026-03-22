using Managers;
using UnityEngine;

namespace Objects
{
    public class SceneObject : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            EventBus.Subscribe(EventType.MapCleared, OnAllStagesCleared);
            EventBus.Subscribe(EventType.TransitionToLobby, OnAllStagesCleared);
            EventBus.Subscribe(EventType.Retry, OnAllStagesCleared);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe(EventType.MapCleared, OnAllStagesCleared);
            EventBus.Unsubscribe(EventType.TransitionToLobby, OnAllStagesCleared);
            EventBus.Unsubscribe(EventType.Retry, OnAllStagesCleared);
        }

        // AllStagesCleared 시 씬 오브젝트를 풀로 반환
        protected virtual void OnAllStagesCleared()
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
