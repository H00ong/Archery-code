using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -7f);
    [SerializeField] private float horizontalSmoothTime = 0.2f;
    [SerializeField] private float verticalSmoothTime = 0.05f;

    [Header("Look-Ahead Settings")]
    [SerializeField] private float lookAheadDistance = 1.5f;
    [SerializeField] private float lookAheadSmoothTime = 0.25f;

    [Header("Test")]
    [SerializeField] private bool useSimpleFollow = false;

    private Transform _target;
    private float _xVelocity;
    private float _yVelocity;
    private float _zVelocity;
    private Vector3 _lookAheadOffset;
    private Vector3 _lookAheadVelocity;

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

    /// <summary>
    /// 스테이지 전환 시 즉시 카메라 위치를 세팅 (스무스 없이 즉시 이동)
    /// </summary>
    public void SetPosition(Vector3 playerPosition)
    {
        transform.position = playerPosition + offset;
        _xVelocity = 0f;
        _lookAheadOffset = Vector3.zero;
        _lookAheadVelocity = Vector3.zero;
    }

    /// <summary>
    /// 추적 대상을 설정. PlayerController의 Transform을 넘기면 됨.
    /// </summary>
    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        if (useSimpleFollow)
        {
            // 메서드 2: 단순히 playerPosition + offset
            transform.position = _target.position + offset;
            return;
        }

        // 메서드 1: SmoothDamp + LookAhead (기존 방식)
        Vector3 targetLookAhead = Vector3.zero;
        var rb = _target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVelocity.sqrMagnitude > 0.01f)
            {
                targetLookAhead = flatVelocity.normalized * lookAheadDistance;
            }
        }

        _lookAheadOffset = Vector3.SmoothDamp(
            _lookAheadOffset, targetLookAhead, ref _lookAheadVelocity, lookAheadSmoothTime);

        // 모든 축에 SmoothDamp 적용 (Y/Z는 짧은 smoothTime으로 거의 즉시 추적하되 떨림 방지)
        float targetX = _target.position.x + offset.x + _lookAheadOffset.x;
        float targetY = _target.position.y + offset.y;
        float targetZ = _target.position.z + offset.z + _lookAheadOffset.z;

        float smoothX = Mathf.SmoothDamp(transform.position.x, targetX, ref _xVelocity, horizontalSmoothTime);
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref _yVelocity, verticalSmoothTime);
        float smoothZ = Mathf.SmoothDamp(transform.position.z, targetZ, ref _zVelocity, verticalSmoothTime);

        transform.position = new Vector3(smoothX, smoothY, smoothZ);
    }
}
