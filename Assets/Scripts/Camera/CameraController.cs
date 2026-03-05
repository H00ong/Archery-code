using UnityEngine;

// TODO: 플레이어를 follow 하는 맵의 설정 정보가 추가되어야 함.
// TODO: 맵마다 카메라의 offset이 달라질 수 있음. (맵 데이터에 offset 정보 추가 필요)
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Vector3 offset;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetPosition(Vector3 playerPosition)
    {
        transform.position = offset;
    }
}
