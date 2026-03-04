using Players;
using UnityEngine;

public class OrbPivot : MonoBehaviour
{
    private Transform _pivot;

    private void Update()
    {
        if (PlayerController.Instance == null)
            return;
        
        _pivot ??= PlayerController.Instance.transform;
        transform.position = new Vector3(_pivot.position.x, transform.position.y, _pivot.position.z);
    }
}
