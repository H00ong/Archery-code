using Game.Enemies.Enum;
using Game.Player;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerData Data { get; private set; }
    public PlayerMovement Move { get; private set; }
    public PlayerAttack Attack { get; private set; }
    public PlayerHurt Hurt { get; private set; } 
    public PlayerSkill Skill { get; private set; }

    public static PlayerState CurrentState = PlayerState.Idle;

    public static bool IsPlayerDead => CurrentState == PlayerState.Dead;
    public Animator anim { get; private set; }


    protected readonly Dictionary<PlayerState, int> animBool = new()
    {
        { PlayerState.Idle,   Animator.StringToHash("Idle")   },
        { PlayerState.Attack, Animator.StringToHash("Attack") },
        { PlayerState.Dead,   Animator.StringToHash("Dead")   },
        { PlayerState.Move,   Animator.StringToHash("Move")   },
    };

    private void Awake()
    {
        InitComponent();
    }

    private void InitComponent()
    {
        anim = GetComponentInChildren<Animator>();
        if (Data == null)   Data = GetComponent<PlayerData>();
        if (Move == null)   Move = GetComponent<PlayerMovement>();
        if (Attack == null) Attack = GetComponent<PlayerAttack>();  Attack.Init();
        if (Hurt == null)   Hurt = GetComponent<PlayerHurt>();      Hurt.Init();
        if (Skill == null)  Skill = GetComponent<PlayerSkill>();    Skill.Init();
    }

    public void ChangePlayerState(PlayerState _newState) 
    {
        UpdateAnimation(_newState);

        CurrentState = _newState;
    }

    public void UpdateAnimation(PlayerState _newState)
    {
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
