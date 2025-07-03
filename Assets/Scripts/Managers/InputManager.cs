using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerInput playerInput;

    public Action OnMoved;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();

        playerInput.Player.Move.performed += ctx => OnMoved?.Invoke();
    }
}
