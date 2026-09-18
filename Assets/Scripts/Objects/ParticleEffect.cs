using System.Collections;
using Managers;
using UnityEngine;

namespace Objects
{
    public class ParticleEffect : SceneObject
    {
        private Coroutine _playCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus.Subscribe(EventType.StageCleared, OnStageCleared);
        }

        protected override void OnDisable()
        {
            EventBus.Unsubscribe(EventType.StageCleared, OnStageCleared);
            _playCoroutine = null;
            base.OnDisable();
        }

        public void InitializeEffect(Vector3 pos) 
        {
            gameObject.transform.position = pos + Vector3.up * 1f;
            gameObject.transform.rotation = Quaternion.identity;

            gameObject.SetActive(true);

            ParticleSystem particle = GetComponent<ParticleSystem>();

            if (!particle) 
            {
                Debug.LogError("Particle is null");
                return;
            }

            _playCoroutine = StartCoroutine(PlayCoroutine(particle));
        }

        // 스테이지가 넘어갈 때 재생 중인 이펙트를 풀로 즉시 회수한다. ReturnObject는 activeSelf 가드가 있어 코루틴과 중복 반환되어도 안전하다.
        private void OnStageCleared()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }

            PoolManager.Instance.ReturnObject(gameObject);
        }

        IEnumerator PlayCoroutine(ParticleSystem ps)
        {
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration);
            ps.Stop();

            _playCoroutine = null;
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
