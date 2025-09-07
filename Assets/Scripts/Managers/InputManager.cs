using Game.Player;
using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] PlayerManager playerManager;
    [HideInInspector] public Vector2 moveInput;
    PlayerInput playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInput = new PlayerInput();
    }

    private void Update()
    {
        if (playerInput.Player.enabled) 
        {
            moveInput = playerInput.Player.Move.ReadValue<Vector2>();

            if (moveInput != Vector2.zero)
            {
                playerManager.Move.Move(moveInput);
            }
            else
            {
                if (EnemyManager.enemies.Count > 0)
                {
                    playerManager.Attack.Attack();
                    playerManager.ChangePlayerState(PlayerState.Attack, PlayerAttack.playerAttackSpeed);
                }
                else
                {
                    playerManager.ChangePlayerState(PlayerState.Idle);
                }
            }
        }
    }

    private void OnEnable()
    {
        ActivePlayerInput(true);
    }
    private void OnDisable()
    {
        ActivePlayerInput(false);
    }

    private void ActivePlayerInput(bool active)
    {
        if (active) playerInput.Player.Enable();
        else playerInput.Player.Disable();
    }

    

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerManager == null) playerManager = FindAnyObjectByType<PlayerManager>();
    }
#endif
}
