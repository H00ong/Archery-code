using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerInput playerInput;

    public Vector2 moveInput;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Update()
    {
        moveInput = playerInput.Player.Move.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        { 
            GameManager.Instance.PlayerManager.PlayerMovement.Move(moveInput);
        }
        else 
        {
            if (EnemyManager.enemies.Count > 0)
            {
                GameManager.Instance.PlayerManager.PlayerAttack.Attack();
                GameManager.Instance.PlayerManager.ChangePlayerState(PlayerState.Attack, PlayerAttack.playerAttackSpeed);
            }
            else 
            {
                GameManager.Instance.PlayerManager.ChangePlayerState(PlayerState.Idle);
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
}
