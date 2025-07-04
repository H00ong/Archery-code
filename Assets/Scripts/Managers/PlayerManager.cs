using UnityEngine;

public enum PlayerState
{
    Idle,
    Move,
    Attack,
    Hurt,
    Dead
}

public class PlayerManager : MonoBehaviour
{
    public PlayerData PlayerData { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }


    void Start()
    {   
        PlayerData = GetComponent<PlayerData>();
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerAttack = GetComponent<PlayerAttack>();
    }   

    void Update()
    {
        
    }

    public void ChangePlayerState(PlayerState _newState, float animSpeed = 1f) 
    {
        if (CurrentState == _newState)
            return;

        UpdateAnimation(_newState, animSpeed);

        CurrentState = _newState;
    }

    public void UpdateAnimation(PlayerState _newState, float animSpeed)
    {
        anim.speed = animSpeed;

        anim.SetBool(CurrentState.ToString(), false);
        anim.SetBool(_newState.ToString(), true);
    }
}
