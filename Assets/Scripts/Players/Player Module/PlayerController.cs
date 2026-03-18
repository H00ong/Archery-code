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

        public PlayerStat Stat { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public PlayerAttack Attack { get; private set; }
        public PlayerHurt Hurt { get; private set; }
        public PlayerSkill Skill { get; private set; }
        public Health Health { get; private set; }
        public Animator Anim { get; private set; }

        public bool IsPlayerDead => currentState == PlayerState.Dead;

        private void Awake()
        {
            SetupSingleton();
            InitComponent();
        }

        void OnEnable()
        {
            EventBus.Subscribe(EventType.TransitionToLobby, ResetGameBuffStat);
            EventBus.Subscribe(EventType.Retry, ResetGameBuffStat);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(EventType.TransitionToLobby, ResetGameBuffStat);
            EventBus.Unsubscribe(EventType.Retry, ResetGameBuffStat);
        }

        public void InitModule()
        {
            currentState = PlayerState.Idle;
            Health.InitializeHealth(Stat.MaxHP);
            Attack.Init();
            Movement.ResetState();
            Skill.Init();
            Hurt.Init();
        }

        private void SetupSingleton()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitComponent()
        {
            Anim = GetComponentInChildren<Animator>();
            Attack ??= GetComponent<PlayerAttack>();
            Movement ??= GetComponent<PlayerMovement>();
            Hurt ??= GetComponent<PlayerHurt>();
            Skill ??= GetComponent<PlayerSkill>();
            Stat ??= GetComponent<PlayerStat>();
            Health ??= GetComponent<Health>();
        }

        public void ChangePlayerAnimation(PlayerState newState)
        {
            UpdateAnimation(newState);

            currentState = newState;
        }

        public void UpdateAnimation(PlayerState newState)
        {
            Anim.SetBool(animBoolHashes[currentState], false);
            Anim.SetBool(animBoolHashes[newState], true);
        }

        public void ResetGameBuffStat()
        {
            Stat.ResetInGameStats();
        }
    }
}
