using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;
    [HideInInspector] public Vector2 moveInput;
    PlayerInput playerInput;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Update()
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

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerManager == null) playerManager = FindAnyObjectByType<PlayerManager>();
    }
#endif
}
