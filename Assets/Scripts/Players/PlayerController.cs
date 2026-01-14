using System.Collections.Generic;
using Game.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Players
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        private readonly Dictionary<PlayerState, int> animBoolHashes = new()
        {
            { PlayerState.Idle,   Animator.StringToHash("Idle")   },
            { PlayerState.Attack, Animator.StringToHash("Attack") },
            { PlayerState.Dead,   Animator.StringToHash("Dead")   },
            { PlayerState.Move,   Animator.StringToHash("Move")   },
        };

        [HideInInspector] public PlayerState currentState = PlayerState.Idle;
        
        public PlayerData Data { get; private set; }
        public PlayerMovement Move { get; private set; }
        public PlayerAttack Attack { get; private set; }
        public PlayerHurt Hurt { get; private set; } 
        public PlayerSkill Skill { get; private set; }

        public bool IsPlayerDead => currentState == PlayerState.Dead;
        public Animator Anim { get; private set; }
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                InitComponent();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            Attack.Init();
            Skill.Init();
        }

        private void InitComponent()
        {
            Anim = GetComponentInChildren<Animator>();
            
            if (Data == null)   Data = GetComponent<PlayerData>();
            if (Move == null)   Move = GetComponent<PlayerMovement>();
            if (Attack == null) Attack = GetComponent<PlayerAttack>();  
            if (Hurt == null)   Hurt = GetComponent<PlayerHurt>();      
            if (Skill == null)  Skill = GetComponent<PlayerSkill>();   
        }

        public void ChangePlayerState(PlayerState newState) 
        {
            UpdateAnimation(newState);

            currentState = newState;
        }

        public void UpdateAnimation(PlayerState newState)
        {
            Anim.SetBool(animBoolHashes[currentState], false);
            Anim.SetBool(animBoolHashes[newState], true);
        }

        public void TakeDamage(int damage) 
        {
            Hurt.TakeDamage(damage);
        }

        public void TakeHeal(int amount, out bool valid) 
        {
            Hurt.TakeHeal(amount, out valid);
        }
    }
}
