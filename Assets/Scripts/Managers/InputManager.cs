using Managers;
using Players;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [HideInInspector] public Vector2 MoveInput;
    private PlayerInput _playerInput;
    private PlayerController _player;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _playerInput = new PlayerInput();
        _player = PlayerController.Instance;
    }

    private void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        if (_playerInput.Player.enabled)
        {
            MoveInput = _playerInput.Player.Move.ReadValue<Vector2>();

            if (MoveInput != Vector2.zero)
            {
                _player.Move.Move(MoveInput);
            }
            else
            {
                if (EnemyManager.Instance.Enemies.Count > 0)
                {
                    _player.Attack.Attack();
                    _player.ChangePlayerState(PlayerState.Attack);
                }
                else
                {
                    _player.ChangePlayerState(PlayerState.Idle);
                }
            }
        }
    }

    private void OnEnable()
    {
        ActivePlayerInput();

        EventBus.Subscribe(EventType.LevelUp, DeactivePlayerInput);
        EventBus.Subscribe(EventType.SkillChosen, ActivePlayerInput);
    }

    private void OnDisable()
    {
        DeactivePlayerInput();

        EventBus.Unsubscribe(EventType.LevelUp, DeactivePlayerInput);
        EventBus.Unsubscribe(EventType.SkillChosen, ActivePlayerInput);
    }

    private void ActivePlayerInput() => _playerInput.Player.Enable();
    private void DeactivePlayerInput() => _playerInput.Player.Disable();
}
