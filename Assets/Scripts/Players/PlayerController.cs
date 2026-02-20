using System.Collections.Generic;
using Stat;
using UnityEngine;

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
        
        /// <summary> 플레이어 3단 레이어 스탯 (Base + Equipment + InGameBuff) </summary>
        public PlayerStat Stat { get; private set; }
        
        public PlayerData Data { get; private set; }
        public PlayerMovement Move { get; private set; }
        public PlayerAttack Attack { get; private set; }
        public PlayerHurt Hurt { get; private set; }
        public PlayerSkill Skill { get; private set; }
        public Health Health { get; private set; }
        public Animator Anim { get; private set; }

        public bool IsPlayerDead => currentState == PlayerState.Dead;
        
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
            Health.InitializeHealth(Stat.MaxHP);
        }

        private void InitComponent()
        {
            Anim = GetComponentInChildren<Animator>();
            Data ??= GetComponent<PlayerData>();
            Move ??= GetComponent<PlayerMovement>();
            Attack ??= GetComponent<PlayerAttack>();
            Hurt ??= GetComponent<PlayerHurt>();
            Skill ??= GetComponent<PlayerSkill>();
            Stat ??= GetComponent<PlayerStat>();
            Health ??= GetComponent<Health>();
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
    }
}
