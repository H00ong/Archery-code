using UnityEngine;

public class OrbPivot : MonoBehaviour
{
    [SerializeField] Transform pivot;

    private void Start()
    {
        if(pivot == null)
            pivot = FindAnyObjectByType<PlayerManager>().transform;
    }

    private void Update()
    {
        if (pivot != null)
        {
            transform.position = new Vector3(pivot.position.x, transform.position.y, pivot.position.z);
        }
    }
}
