using Managers;
using UnityEngine;

namespace Objects
{
    public class SceneObject : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            EventBus.Subscribe(EventType.MapCleared, OnAllStagesCleared);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe(EventType.MapCleared, OnAllStagesCleared);
        }

        // AllStagesCleared 시 씬 오브젝트를 풀로 반환
        protected virtual void OnAllStagesCleared()
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
