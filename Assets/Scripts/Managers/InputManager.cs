using Game.Player;
using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] PlayerManager _player;
    [HideInInspector] public Vector2 MoveInput;
    PlayerInput _playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerInput = new PlayerInput();
    }

    private void Update()
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
                if (EnemyManager.Enemies.Count > 0)
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
        ActivePlayerInput(true);

        LevelUpFlow.OnLevelUp += () => ActivePlayerInput(false);
        LevelUpFlow.OnSkillChosen += () => ActivePlayerInput(true);
    }

    private void OnDisable()
    {
        ActivePlayerInput(false);

        LevelUpFlow.OnLevelUp -= () => ActivePlayerInput(false);
        LevelUpFlow.OnSkillChosen -= () => ActivePlayerInput(true);
    }

    private void ActivePlayerInput(bool active)
    {
        if (active) _playerInput.Player.Enable();
        else _playerInput.Player.Disable();
    }

    

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_player == null) _player = FindAnyObjectByType<PlayerManager>();
    }
#endif
}
