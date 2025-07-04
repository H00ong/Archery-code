using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Vector3 Offset;
    void Start()
    {
        Offset = transform.position;
    }

    private void LateUpdate()
    {
        transform.position = GameManager.Instance.PlayerManager.PlayerMovement.transform.position + Offset;
    }
}
