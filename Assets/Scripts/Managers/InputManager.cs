using Managers;
using Players;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO : Event Bus로 플레이어 입력 관리 전환
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private PlayerInput _playerInput;
    public Vector2 MoveInput { get; private set; }

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

    // 이동 입력값이 들어올 때만 호출됨
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    // 이동 입력이 0으로 돌아올 때 한 번만 호출됨
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MoveInput = Vector2.zero;
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
        EventBus.Subscribe(EventType.MapCleared, DeactivePlayerInput);
        EventBus.Subscribe(EventType.PlayerDied, DeactivePlayerInput);
        EventBus.Subscribe(EventType.Retry, DeactivePlayerInput);
        EventBus.Subscribe(EventType.TransitionToLobby, DeactivePlayerInput);
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
        EventBus.Unsubscribe(EventType.MapCleared, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.PlayerDied, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.Retry, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.TransitionToLobby, DeactivePlayerInput);
    }

    private void ActivePlayerInput() => _playerInput.Player.Enable();
    private void DeactivePlayerInput() => _playerInput.Player.Disable();
}
