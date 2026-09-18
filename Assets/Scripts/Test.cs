using System.Collections.Generic;
using System.Linq; // ★ LINQ 필수!
using UnityEngine; // ★ Unity 필수

public class Test : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float rb1Speeds = 7f;
    [SerializeField] private Rigidbody rb2;
    [SerializeField] private float rb2Speeds = 9f;

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.forward * rb1Speeds;
        }

        if (rb2 != null)
        {
            rb2.linearVelocity = Vector3.forward * rb2Speeds;
        }
    }
}
