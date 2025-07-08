using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicBeam_Player : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Enemy")) 
        {
            Destroy(gameObject);
        }
    }
}
