using UnityEngine;

public enum PlayerState
{
    Idle,
    Move,
    Attack,
    Dead
}

public class PlayerManager : MonoBehaviour
{
    public PlayerData PlayerData { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }
    public PlayerHeatlh PlayerHeatlh { get; private set; }
    

    public static PlayerState CurrentState = PlayerState.Idle;
    public static bool IsPlayerDead => CurrentState == PlayerState.Dead;
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
        PlayerHeatlh = GetComponent<PlayerHeatlh>();
    }   

    void Update()
    {
        
    }

    public void ChangePlayerState(PlayerState _newState, float animSpeed = 1f) 
    {
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
