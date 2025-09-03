using System.Collections;
using TMPro;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    public void SetupEffect(Vector3 _pos) 
    {
        gameObject.transform.position = new Vector3(_pos.x, 0.1f, _pos.z);
        gameObject.transform.rotation = Quaternion.identity;

        gameObject.SetActive(true);

        ParticleSystem particle = GetComponent<ParticleSystem>();

        if (particle == null) 
        {
            Debug.LogError("Particle is null");
            return;
        }

        StartCoroutine(PlayCoroutine(particle));
    }

    IEnumerator PlayCoroutine(ParticleSystem _ps)
    {
        _ps.Play();
        yield return new WaitForSeconds(_ps.main.duration);
        _ps.Stop();

        PoolManager.Instance.ReturnObject(gameObject);
    }
}
