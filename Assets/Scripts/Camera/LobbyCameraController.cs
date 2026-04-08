using UnityEngine;

public class LobbyCameraController : MonoBehaviour
{
    [Header("카메라 이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;

    [Header("Lock Icon")]
    [SerializeField] private GameObject lockIcon;

    private float _yOffset;
    private int _maxReachableIndex;
    private int _totalMapCount;

    private int _nextMapIndex;

    private int _currentStageIndex;
    private float _targetY;
    private float _baseY;
    private bool _hasTarget;

    private Vector3 originalPosition;

    public bool MapSelectInputBlocked { get; set; }

    public void Setup(int totalMapCount, float yOffset, int nextMapIndex, Vector3 initPos)
    {
        originalPosition = initPos;

        transform.position = initPos + cameraOffset;
        _totalMapCount = totalMapCount;
        _yOffset = yOffset;
        _nextMapIndex = nextMapIndex;

        _maxReachableIndex = totalMapCount - 1;
        
        _baseY = transform.position.y;
        SetStage(nextMapIndex);
    }

    void Update()
    {
        if (_totalMapCount <= 0 || MapSelectInputBlocked) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangeStage(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangeStage(-1);

        if (!_hasTarget) return;

        Vector3 targetPosition = new Vector3(
            transform.position.x,
            _targetY,
            transform.position.z);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            _hasTarget = false;
        }
    }

    public void ChangeStage(int direction)
    {
        _currentStageIndex += direction;
        _currentStageIndex = Mathf.Clamp(_currentStageIndex, 0, _maxReachableIndex);
        _targetY = _baseY + (_currentStageIndex * _yOffset);
        _hasTarget = true;
        UpdateLockIcon();
    }

    public void SetStage(int index)
    {
        _currentStageIndex = Mathf.Clamp(index, 0, _maxReachableIndex);

        if (index > 0)
        {
            transform.position += new Vector3(0, _currentStageIndex * _yOffset, 0);
        }
        
        _hasTarget = false;
        UpdateLockIcon();
    }

    private void UpdateLockIcon()
    {
        if (lockIcon != null)
            lockIcon.SetActive(_currentStageIndex > _nextMapIndex);
    }

    public int CurrentStageIndex => _currentStageIndex;

    [ContextMenu("Apply Offset To Position")]
    private void ApplyOffsetToPosition()
    {
        transform.position = originalPosition + cameraOffset;
    }
}
