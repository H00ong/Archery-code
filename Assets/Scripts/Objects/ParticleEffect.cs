using System.Collections;
using Managers;
using UnityEngine;

namespace Objects
{
    public class ParticleEffect : MonoBehaviour
    {
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

            StartCoroutine(PlayCoroutine(particle));
        }

        IEnumerator PlayCoroutine(ParticleSystem ps)
        {
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration);
            ps.Stop();

            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
