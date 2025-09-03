using Game.Enemies.Enum;
using System.Collections.Generic;
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
    public PlayerData Data { get; private set; }
    public PlayerMovement Move { get; private set; }
    public PlayerAttack Attack { get; private set; }
    public PlayerHurt Hurt { get; private set; }
    
    

    public static PlayerState CurrentState = PlayerState.Idle;

    public static bool IsPlayerDead => CurrentState == PlayerState.Dead;
    Animator anim;

    protected readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
    protected readonly int AttackIndex = Animator.StringToHash("AttackIndex");
    protected readonly Dictionary<PlayerState, int> animBool = new()
    {
        { PlayerState.Idle,   Animator.StringToHash("Idle")   },
        { PlayerState.Attack, Animator.StringToHash("Attack") },
        { PlayerState.Dead,   Animator.StringToHash("Dead")   },
        { PlayerState.Move,   Animator.StringToHash("Move")   },
    };

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {   
        Data = GetComponent<PlayerData>();
        Move = GetComponent<PlayerMovement>();
        Attack = GetComponent<PlayerAttack>(); Attack.Init();
        Hurt = GetComponent<PlayerHurt>(); Hurt.Init();
    }

    public void ChangePlayerState(PlayerState _newState, float animSpeed = 1f) 
    {
        UpdateAnimation(_newState, animSpeed);

        CurrentState = _newState;
    }

    public void UpdateAnimation(PlayerState _newState, float animSpeed)
    {
        anim.speed = animSpeed;

        anim.SetBool(animBool[CurrentState], false);
        anim.SetBool(animBool[_newState], true);
    }

    public void GetHit(int damage) 
    {
        Hurt.GetHit(damage);
    }

    public void GetHeal(int amount, out bool valid) 
    {
        Hurt.GetHeal(amount, out valid);
    }
}
