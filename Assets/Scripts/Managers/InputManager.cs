using Managers;
using Players;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO : Event Bus로 플레이어 입력 관리 전환
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private Vector2 _moveInput;
    private PlayerInput _playerInput;
    private PlayerController _player;
    private PlayerController Player => _player ??= PlayerController.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _playerInput = new PlayerInput();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        HandlePlayerMovement();
    }

    private void HandlePlayerMovement()
    {
        if(Player == null)
            return;

        if (_moveInput != Vector2.zero)
        {
            Player.Movement.ExecuteMovement(_moveInput);
        }
        else
        {
            if (EnemyManager.Instance.Enemies.Count > 0)
            {
                Player.ChangePlayerState(PlayerState.Attack);
                Player.Attack.Attack();
            }
            else
            {
                Player.ChangePlayerState(PlayerState.Idle);
            }
        }
    }

    // 이동 입력값이 들어올 때만 호출됨
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    // 이동 입력이 0으로 돌아올 때 한 번만 호출됨
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
            _moveInput = Vector2.zero;

        if (EnemyManager.Instance.Enemies.Count > 0)
        {
            Player.Attack.Attack();
            Player.ChangePlayerState(PlayerState.Attack);
        }
        else
        {
            Player.ChangePlayerState(PlayerState.Idle);
        }
    }

    private void OnEnable()
    {
        DeactivePlayerInput();

        _playerInput.Player.Move.performed += OnMovePerformed;
        _playerInput.Player.Move.canceled  += OnMoveCanceled;

        EventBus.Subscribe(EventType.StageCombatStarted, ActivePlayerInput);
        EventBus.Subscribe(EventType.StageLoadingStarted, DeactivePlayerInput);
        EventBus.Subscribe(EventType.LevelUp, DeactivePlayerInput);
        EventBus.Subscribe(EventType.SkillChosen, ActivePlayerInput);
    }

    private void OnDisable()
    {
        DeactivePlayerInput();

        _playerInput.Player.Move.performed -= OnMovePerformed;
        _playerInput.Player.Move.canceled  -= OnMoveCanceled;

        EventBus.Unsubscribe(EventType.StageCombatStarted, ActivePlayerInput);
        EventBus.Unsubscribe(EventType.StageLoadingStarted, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.LevelUp, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.SkillChosen, ActivePlayerInput);
    }

    private void ActivePlayerInput() => _playerInput.Player.Enable();
    private void DeactivePlayerInput() => _playerInput.Player.Disable();
}
