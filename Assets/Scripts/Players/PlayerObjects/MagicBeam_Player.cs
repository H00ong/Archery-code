using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicBeam_Player : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) 
        {
            Destroy(gameObject);
        }
    }
}
