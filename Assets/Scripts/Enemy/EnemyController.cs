using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Map;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Enemy
{
    public class EnemyController : MonoBehaviour
    {
        public static readonly Dictionary<EnemyState, int> StateAnimHashes = new()
        {
            { EnemyState.Idle, Animator.StringToHash("Idle") },
            { EnemyState.Hurt, Animator.StringToHash("Hurt") },
            { EnemyState.Attack, Animator.StringToHash("Attack") },
            { EnemyState.Dead, Animator.StringToHash("Dead") },
            { EnemyState.Move, Animator.StringToHash("Move") },
        };

        private const float EnemyReturnDelay = .25f;

        [Header("Debug")] 
        public bool isDebugMode = true;

        [Header("Identity")] 
        [SerializeField] public EnemyName enemyName;
        [SerializeField] public EnemyTag enemyTags;
        private EnemyStat _stat;
        
        public EnemyStat stat => _stat;
        public EnemyState CurrentState { get; private set; }
        public bool IsBoss => EnemyTagUtil.Has(enemyTags, EnemyTag.Boss);

        [Header("Modules")]
        public EnemyBrain brain;
        [SerializeField] private EnemyIdle idle;
        [SerializeField] private EnemyMove move;
        [SerializeField] private EnemyHurt hurt;
        [SerializeField] private EnemyDie die;

        private readonly List<EnemyAttack> _attacks = new List<EnemyAttack>();
        private readonly Dictionary<EnemyTag, IEnemyBehavior> _modules = new Dictionary<EnemyTag, IEnemyBehavior>();
        
        [Header("Components")]
        public Health health;
        public Animator anim;
        public Rigidbody rigidBody;
        public Collider enemyCollider;
        public EnemyReferenceHub enemyReference;
        public EnemyVisual enemyVisual;

        [Header("Idle Tuning")]
        [SerializeField] private float defaultIdleTime = 2f;

        
        [Header("Drop Item")]
        [SerializeField] public AssetReferenceGameObject expItemPrefab;

        [Header("Default Tuning")]
        [SerializeField] private float defaultAttackSpeed;


        [HideInInspector] public Vector3 lastPlayerPosition;
        [HideInInspector] public PlayerController player;

        public bool AttackMoveTrigger { get; set; }
        public bool HurtEndTrigger { get; set; }
        public bool AttackEndTrigger { get; set; }
        public bool HasMultiAttackModules => _attacks.Count > 1;
    
        private Action OnEnter, OnExit, OnTick;
        private Dictionary<EnemyState, (Action enter, Action exit, Action tick)> ActionTable;

        #region Unity Cycle

        void Update()
        {
            if (AttackEndTrigger)
            {
                OnModuleComplete();
            }
        }

        private void FixedUpdate()
        {
            OnTick?.Invoke();
        }

        private void OnDisable()
        {
            ClearAction();
        }
        #endregion

        #region Initialization
        public void InitializeEnemy(EnemyIdentity identity = null)
        {
            CacheComponent();

            // EnemyIdentity가 제공되면 tag와 visual 주입
            if (identity != null)
            {
                SetIdentity(identity);
            }

            player = PlayerController.Instance;

            RigidbodyActive(true);
            ColliderActive(true);
            SetStat();
            InitModule();

            anim.SetFloat(AnimHashes.AttackSpeed, defaultAttackSpeed);
            InitState(EnemyState.Idle);
        }
        
        private void SetIdentity(EnemyIdentity identity)
        {
            // Tag 주입
            enemyTags = identity.Tag;
            
            // Visual 주입
            if (enemyVisual != null)
            {
                enemyVisual.ApplyMaterials(identity.ObjectMat, identity.AccessoryMat);
                enemyVisual.Initialize();
            }
        }

        private void CacheComponent()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!rigidBody) rigidBody = GetComponent<Rigidbody>();
            if (!enemyCollider) enemyCollider = GetComponentInChildren<Collider>();
            if (!health) health = GetComponent<Health>();
            if (!enemyReference) enemyReference = GetComponent<EnemyReferenceHub>();
            if (!enemyVisual) enemyVisual = GetComponent<EnemyVisual>();
        }

        private void SetStat()
        {
            if (!isDebugMode)
            {
                _stat = gameObject.GetOrAddComponent<EnemyStat>();
                EnemyManager.Instance.SetEnemyStat(enemyName, enemyTags, _stat);
                return;
            }
        }

        public void ColliderActive(bool active) => enemyCollider.enabled = active;

        public void RigidbodyActive(bool active)
        {
            if (rigidBody)
            {
                rigidBody.isKinematic = !active;
                rigidBody.constraints = RigidbodyConstraints.FreezeRotation |
                                        RigidbodyConstraints.FreezePositionY;
            }
        }

        private void InitMoveModule()
        {
            move = EnemyBehaviorFactory.CreateMoveModules(this, enemyName, enemyTags, _modules);
        }

        private void InitAttackModule()
        {
            EnemyBehaviorFactory.CreateAttackModules(this, enemyName, enemyTags, _modules, _attacks);
        }

        private void ClearAction()
        {
            OnEnter = null;
            OnTick = null;
            OnExit = null;

            health.OnDie -= this.OnDie;
            health.OnHit -= this.OnHit;
        }

        private void InitModule()
        {
            ClearAction();

            enemyReference.Init();

            _attacks.Clear();
            _modules.Clear();

            InitMoveModule();
            InitAttackModule();

            idle = gameObject.GetOrAddComponent<EnemyIdle>();
            idle.SetIdleTime(defaultIdleTime);
            idle.Init(this);
            
            die = gameObject.GetOrAddComponent<EnemyDie>();   die.Init(this);
            hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);
            health = gameObject.GetOrAddComponent<Health>();  health.InitializeHealth(stat.MaxHP);

            health.OnHit += this.OnHit;
            health.OnDie += this.OnDie;

            ActionTable = new Dictionary<EnemyState, (Action enter, Action exit, Action tick)> {
                { EnemyState.Idle,   (enter: idle.OnEnter,        exit: idle.OnExit,                tick: idle.Tick) },
                { EnemyState.Move,   (enter: move.OnEnter,        exit: move.OnExit,                tick: move.Tick) },
                { EnemyState.Attack, (enter: _attacks[0].OnEnter,  exit: _attacks[0].OnExit,          tick: _attacks[0].Tick)},
                { EnemyState.Hurt,   (enter: hurt.OnEnter,        exit: hurt.OnExit,                tick: hurt.Tick)},
                { EnemyState.Dead,   (enter: die.OnEnter,         exit: die.OnExit,                 tick: die.Tick)},
            };
        }
        #endregion

        #region Overridable Methods
        public int GetAtk()
        {
            return _stat.AttackPower;
        }
        #endregion

        #region Animation Events

        public void Ability(EnemyTag attackTypeTag)
        {
            if (!_modules.TryGetValue(attackTypeTag, out var attack)) return;
            if (attack is not EnemyAttack enemyAttack) return;
            
            enemyAttack.OnAnimEvent();
        }
    
        public void SetHurtEndTrigger(bool active) => HurtEndTrigger = active;
        public void SetAttackEndTrigger(bool active) => AttackEndTrigger = active;
        public void SetAttackMoveTrigger(bool active) => AttackMoveTrigger = active;

        public async void Die()
        {
            await Awaitable.WaitForSecondsAsync(EnemyReturnDelay, destroyCancellationToken);

            PoolManager.Instance.ReturnObject(gameObject);
        }

        #endregion

        #region State Methods

        public void OnModuleComplete()
        {
            if (CurrentState == EnemyState.Dead) return;
            
            ChangeState(brain.GetNextAction(CurrentState));
        }


        private void InitState(EnemyState initState) 
        {
            CurrentState = initState;
            (OnEnter, OnExit, OnTick) = ActionTable[initState];
            OnEnter?.Invoke();
        }

        public void ChangeState(EnemyState next)
        {
            if (CurrentState == next) return;
            if (CurrentState == EnemyState.Dead) return;
            
            OnExit?.Invoke();
            anim.SetBool(StateAnimHashes[CurrentState], false);
            
            CurrentState = next;
            
            if (CurrentState == EnemyState.Attack)
            {
                ChooseAttackModule();
            }

            (OnEnter, OnExit, OnTick) = ActionTable[next];
            OnEnter?.Invoke();
            anim.SetBool(StateAnimHashes[next], true);
        }
        
        private void ChooseAttackModule()
        {
            if (_attacks.Count <= 1) return;

            var currentAttackIndex = UnityEngine.Random.Range(0, _attacks.Count);
            var selectedAttack = _attacks[currentAttackIndex];
            
            ActionTable[EnemyState.Attack] = (selectedAttack.OnEnter, selectedAttack.OnExit, selectedAttack.Tick);
        }
        #endregion

        private void OnHit()
        {
            if (!IsBoss)
            {
                ChangeState(EnemyState.Hurt);
            }
        }

        private void OnDie() => ChangeState(EnemyState.Dead);

        public void ReturnImmediately()
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                if (CurrentState != EnemyState.Move)
                    return;
                
                if(move is RandomMove randomMove)
                {
                    randomMove.PickReflectDirection(collision.GetContact(0).point - transform.position, collision.GetContact(0).normal);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!health) health = GetComponent<Health>();
            if (!enemyCollider) enemyCollider = GetComponentInChildren<Collider>();
            if (!rigidBody) rigidBody = GetComponent<Rigidbody>();
            if (!idle) GetComponent<EnemyIdle>();
            if(!die) GetComponent<EnemyDie>();
            if(!hurt) GetComponent<EnemyHurt>();
            if (!move) GetComponent<EnemyMove>();
        }
#endif
    }
}