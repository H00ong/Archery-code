using Managers;
using Players;
using Stat;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float threshold = 0.01f; // 입력이 너무 작은 경우 무시하기 위한 임계값

    [SerializeField] private Rigidbody playerRigidbody;

    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private PlayerStat _stat;
    private InputManager _inputManager;
    private Vector3 _currentMoveDir; // InputManager로부터 전달받은 이동 방향

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        ApplyRigidbodySettings();
    }

    void Start()
    {
        _stat = PlayerController.Instance.Stat;
        _playerController = PlayerController.Instance;
        _playerAttack = PlayerController.Instance.Attack;
        _inputManager = InputManager.Instance;
    }

    private void ApplyRigidbodySettings()
    {
        playerRigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                          RigidbodyConstraints.FreezeRotation;

        playerRigidbody.mass = 9999f;
    }

    void Update()
    {
        SetMoveInput(_inputManager.MoveInput);
    }

    private void SetMoveInput(Vector2 moveInput)
    {
        _currentMoveDir = new Vector3(moveInput.x, 0, moveInput.y);

        if (_currentMoveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_currentMoveDir, Vector3.up);
            _playerController.ChangePlayerAnimation(PlayerState.Move);
        }
        else
        {
            if (EnemyManager.Instance.Enemies.Count > 0)
            {
                _playerController.ChangePlayerAnimation(PlayerState.Attack);
                _playerAttack.Attack();
            }
            else
            {
                _playerController.ChangePlayerAnimation(PlayerState.Idle);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_currentMoveDir == Vector3.zero)
            return;

        Vector3 targetPos = playerRigidbody.position + _currentMoveDir * _stat.MoveSpeed * Time.fixedDeltaTime;

        playerRigidbody.MovePosition(targetPos);
    }
    
    public void UpdateMoveSpeed(float modifier)
    {
        _stat.ApplyMoveSpeedModifier(modifier);
    }
}