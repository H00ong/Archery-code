using Players;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] PlayerController playerManager;
    Vector3 Offset;

    void Start()
    {
        Offset = transform.position;
    }

    private void LateUpdate()
    {
        transform.position = playerManager.Move.transform.position + Offset;
    }
}
