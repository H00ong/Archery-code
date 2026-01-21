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

        [Header("Debug")] 
        public bool isDebugMode = true;

        [Header("Identity")] 
        [SerializeField] private EnemyName enemyName;
        [SerializeField] public EnemyTag enemyTags;
        public EnemyStats stats;
        public EnemyState CurrentState { get; protected set; }
        public bool IsBoss => EnemyTagUtil.Has(enemyTags, EnemyTag.Boss);

        [Header("Modules")]
        public EnemyBrain brain;
        [SerializeField] protected EnemyIdle idle;
        [SerializeField] protected EnemyMove move;
        [SerializeField] protected EnemyHurt hurt;
        [SerializeField] protected EnemyDie die;

        private readonly List<EnemyAttack> attacks = new List<EnemyAttack>();
        private readonly Dictionary<EnemyTag, IEnemyBehavior> _modules = new Dictionary<EnemyTag, IEnemyBehavior>();
        
        [Header("Components")]
        public Health health;
        public Animator anim;
        public Rigidbody rigidBody;
        public Collider enemyCollider;
        public EnemyReferenceHub enemyReference;

        [Header("Idle Tuning")]
        [SerializeField] protected float defaultIdleTime = 2f;
        
        [Header("Drop Item")]
        [SerializeField] public AssetReferenceGameObject expItemPrefab;
        
        [Header("Default Tuning")]
        [SerializeField] protected float defaultAttackSpeed;
        [SerializeField] protected int defaultAtk;

        [Header("Collision")]
        [SerializeField] protected Transform blockCheck;
        [SerializeField] protected float sphereCastRadius = 0f;
        [SerializeField] protected float castDistance = .5f;
        [SerializeField] protected LayerMask obstacleLayer;
        
        [HideInInspector] public Vector3 lastPlayerPosition;
        [HideInInspector] public PlayerController player;
        
        private EnemyData _enemyData;
        private DataManager _dataManager;

        public bool IsBlocked { get; private set; }
        public bool AttackMoveTrigger { get; set; }
        public bool HurtEndTrigger { get; set; }
        public bool AttackEndTrigger { get; set; }
        public bool HasMultiAttackModules => attacks.Count > 1;
        
    
        protected Action OnEnter, OnExit, OnTick;
        protected Dictionary<EnemyState, (Action enter, Action exit, Action tick)> ActionTable;
    
        #region Unity Cycle

        private void BlockCheck()
        {
            var origin = blockCheck.position;
            var radius = sphereCastRadius;
            var dir = transform.forward;
            var checkDistance = castDistance;
            var mask = obstacleLayer;

            if (Physics.SphereCast(
                    origin,
                    radius,
                    dir,
                    out var hit,
                    checkDistance,
                    mask)) 
            {
                if (move is RandomMove randomMove)
                {
                    randomMove.PickReflectDirection(transform.forward, hit.normal);
                }
            }
            else
            {
                IsBlocked = false;
            }
        }

        protected virtual void Update()
        {
            BlockCheck();

            if (AttackEndTrigger)
            {
                OnModuleComplete();
            }

            OnTick?.Invoke();
        }

        protected virtual void OnDisable()
        {
            ClearAction();
        }
        #endregion

        #region Initialization
        public void InitializeEnemy()
        {
            SetupManager();
            CacheComponent();
            RigidbodyActive(true);
            ColliderActive(true);
            ApplyEnemyData();
            InitModule();

            anim.SetFloat(AnimHashes.AttackSpeed, defaultAttackSpeed);
        
            InitState(EnemyState.Idle);
        }

        private void SetupManager()
        {
            _dataManager = DataManager.Instance;
            player = PlayerController.Instance;
        }

        private void CacheComponent()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!health) health = GetComponent<Health>();
            if (!enemyCollider) enemyCollider = GetComponentInChildren<Collider>();
            if (!rigidBody) rigidBody = GetComponent<Rigidbody>();
            if (!enemyReference) enemyReference = GetComponent<EnemyReferenceHub>();
            
            enemyReference.Init();
        }

        protected virtual void ApplyEnemyData()
        {
            if (!isDebugMode)
            {
                _enemyData = _dataManager.GetEnemyData(enemyName, enemyTags);
                if (_enemyData == null)
                {
                    Debug.LogError("Enemy data is null");
                    return;
                }

                MapData mapData = MapManager.Instance.CurrentMapData;
                if (mapData == null)
                {
                    Debug.LogError("Map data is null");
                    return;
                }

                int stageIndex = StageManager.Instance.CurrentStageIndex;

                stats = EnemyStatUtil.GetEnemyStats(_enemyData, enemyTags, mapData, stageIndex);
                return;
            }
        }

        public void ColliderActive(bool active) => enemyCollider.enabled = active;

        public void RigidbodyActive(bool active)
        {
            if (rigidBody)
            {
                rigidBody.isKinematic = !active;
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        protected virtual void InitMoveModule()
        {
            move = EnemyBehaviorFactory.CreateMoveModules(this, enemyName, enemyTags, _modules);
        }

        protected void InitAttackModule()
        {
            EnemyBehaviorFactory.CreateAttackModules(this, enemyName, enemyTags, _modules, attacks);
        }

        protected void ClearAction()
        {
            OnEnter = null;
            OnTick = null;
            OnExit = null;

            health.OnDie -= this.OnDie;
            health.OnHit -= this.OnHit;
        }

        protected virtual void InitModule()
        {
            ClearAction();
            InitMoveModule();
            InitAttackModule();

            idle = gameObject.GetOrAddComponent<EnemyIdle>();
            idle.SetIdleTime(defaultIdleTime);
            idle.Init(this);
            
            die = gameObject.GetOrAddComponent<EnemyDie>();   die.Init(this);
            hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);
            health = gameObject.GetOrAddComponent<Health>();  health.InitializeHealth(1);

            health.OnHit += this.OnHit;
            health.OnDie += this.OnDie;

            ActionTable = new Dictionary<EnemyState, (Action enter, Action exit, Action tick)> {
                { EnemyState.Idle,   (enter: idle.OnEnter,        exit: idle.OnExit,                tick: idle.Tick) },
                { EnemyState.Move,   (enter: move.OnEnter,        exit: move.OnExit,                tick: move.Tick) },
                { EnemyState.Attack, (enter: attacks[0].OnEnter,  exit: attacks[0].OnExit,          tick: attacks[0].Tick)},
                { EnemyState.Hurt,   (enter: hurt.OnEnter,        exit: hurt.OnExit,                tick: hurt.Tick)},
                { EnemyState.Dead,   (enter: die.OnEnter,         exit: die.OnExit,                 tick: die.Tick)},
            };
        }
        #endregion

        #region Overridable Methods
        public int GetAtk()
        {
            return defaultAtk;
        }
        #endregion

        #region Animation Events

        public void Ability(EnemyTag attackTypeTag)
        {
            if (!_modules.TryGetValue(attackTypeTag, out var attack)) return;
            if (attack is not EnemyAttack enemyAttack) return;
            
            enemyAttack.OnAnimEvent();
        }
    
        public void SetHurtEndTrigger(bool active) 
        {
            HurtEndTrigger = active;
        }

        public void SetAttackEndTrigger(bool active) 
        {
            AttackEndTrigger = active;
        }

        public void SetAttackMoveTrigger(bool active) 
        {
            AttackMoveTrigger = active;
        }

        public void Die() 
        {
            StartCoroutine(ReturnCoroutine());
        }

        private IEnumerator ReturnCoroutine() 
        {
            yield return new WaitForSeconds(.5f);

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

        public virtual void ChangeState(EnemyState next)
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
            if (attacks.Count <= 1) return;

            var currentAttackIndex = 1; //UnityEngine.Random.Range(0, attacks.Count);
            var selectedAttack = attacks[currentAttackIndex];
            
            ActionTable[EnemyState.Attack] = (selectedAttack.OnEnter, selectedAttack.OnExit, selectedAttack.Tick);
        }
        #endregion

        protected virtual void OnHit()
        {
            if (!IsBoss)
            {
                ChangeState(EnemyState.Hurt);
            }
        }

        protected virtual void OnDie() => ChangeState(EnemyState.Dead);


        protected virtual void OnDrawGizmos()
        {
            if (blockCheck == null) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(blockCheck.position, sphereCastRadius);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!health) health = GetComponent<Health>();
            if (!enemyCollider) enemyCollider = GetComponentInChildren<Collider>();
            if (!rigidBody) rigidBody = GetComponent<Rigidbody>();
            if (!idle) GetComponent<EnemyIdle>();
            if(!die) GetComponent<EnemyDie>();
            if(!hurt) GetComponent<EnemyHurt>();
            if(!move) GetComponent<EnemyMove>();
        }
#endif
    }
}