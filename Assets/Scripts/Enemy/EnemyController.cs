using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Managers;
using Map;
using Players;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;

namespace Enemy
{
    public enum EnemyPointType
    {
        FlyingShootingMuzzle,
        NormalShootingMuzzle,
    }
    
    public class EnemyController : MonoBehaviour
    {
        protected static readonly Dictionary<EnemyState, int> StateAnimHashes = new()
        {
            { EnemyState.Idle,   Animator.StringToHash("Idle")   },
            { EnemyState.Hurt,   Animator.StringToHash("Hurt")   },
            { EnemyState.Attack, Animator.StringToHash("Attack") },
            { EnemyState.Dead,   Animator.StringToHash("Dead")   },
            { EnemyState.Move,   Animator.StringToHash("Move")   },
        };
    
        [Header("Debug")]
        public bool isDebugMode = true;

        [Header("Identity")]
        [SerializeField] private EnemyName enemyName;
        [SerializeField] public EnemyTag enemyTags;
        public EnemyStats stats;
        public EnemyState CurrentState { get; protected set; }

        [Header("Modules")]
        [SerializeField] protected EnemyIdle idle;
        [SerializeField] protected EnemyMove move;
        [SerializeField] protected EnemyHurt hurt;
        [SerializeField] protected EnemyDie die;
        public List<EnemyAttack> attacks = new List<EnemyAttack>();

        [Header("Components")]
        public Health health;
        public Animator anim;
        public Rigidbody rigidBody;
        public Collider enemyCollider;
        public EnemyReferenceHub enemyReference;

        [Header("Default Tuning")]
        [SerializeField] protected float defaultAttackSpeed; // attack anim speed;
        [SerializeField] protected int defaultAtk;

        [Header("Collision")]
        [SerializeField] protected Transform blockCheck;
        [SerializeField] protected float sphereCastRadius = 0f;
        [SerializeField] protected float castDistance = .5f;
        [SerializeField] protected LayerMask obstacleLayer;
        
        [HideInInspector] public Vector3 lastPlayerPosition;
        [HideInInspector] public PlayerController player;
        
        private EnemyData _enemyData;
        private PoolManager _poolManager;
        private DataManager _dataManager;

        public bool IsBlocked { get; private set; }
        public bool AttackMoveTrigger { get; set; }
        public bool HurtEndTrigger { get; set; }
        public bool AttackEndTrigger { get; set; }

        private readonly Dictionary<EnemyTag, EnemyAttack> _attackDict = new Dictionary<EnemyTag, EnemyAttack>();
    
        protected Action OnEnter, OnExit, OnTick;
        protected Action OnAttackEnter, OnAttackExit, OnAttackTick;
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
                ChangeState(EnemyState.Idle);
                return;
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
            _poolManager = PoolManager.Instance;
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
            foreach (var (tag, type) in EnemyBehaviorFactory.MoveFactory)
            {
                if (EnemyTagUtil.Has(enemyTags, tag))
                {
                    move = (EnemyMove)gameObject.GetOrAddComponent(type);
                    move.Init(this);
                    break;
                }
            }
        
            if (!move)
            {
                Debug.LogError("No Enemy Move Tag");
            }
        }

        protected void InitAttackModule()
        {
            var relevantData = new Dictionary<EnemyTag, BaseModuleData>();
            EnemyTag myAttributes = enemyTags & EnemyTag.AttributeMask;
            
            foreach (var actionTag in EnemyTagUtil.AllActionTags)
            {
                if (EnemyTagUtil.Has(enemyTags, actionTag))
                {
                    BaseModuleData foundData = null;
                    
                    EnemyTag specificTag = actionTag | myAttributes;
                    var key1 = new EnemyKey(this.enemyName, specificTag);
                    foundData = EnemyManager.Instance.GetModuleData(key1);
                    
                    // 찾은 데이터 등록 (딕셔너리에는 순수 ActionTag를 키로 저장)
                    if (foundData != null)
                    {
                        relevantData.TryAdd(actionTag, foundData);
                    }
                }
            }

            EnemyBehaviorFactory.CreateAttackModules(this, enemyTags, attacks, _attackDict, relevantData);

            foreach (var atk in attacks)
            {
                OnAttackEnter += atk.OnEnter;
                OnAttackTick += atk.Tick;
                OnAttackExit += atk.OnExit;
            }
        }

        protected void ClearAction() 
        {
            OnEnter = null;
            OnTick = null;
            OnExit = null;

            OnAttackEnter = null;
            OnAttackExit = null;
            OnAttackTick = null;

            health.OnDie -= this.OnDie;
            health.OnHit -= this.OnHit;
        }

        protected virtual void InitModule()
        {
            ClearAction();
            InitMoveModule();

            // 기본 모듈 초기화 (기존 코드 유지)
            idle = gameObject.GetOrAddComponent<EnemyIdle>(); idle.Init(this);
            die = gameObject.GetOrAddComponent<EnemyDie>(); die.Init(this);
            hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);
            health = gameObject.GetOrAddComponent<Health>(); health.InitializeHealth(1);

            health.OnHit += this.OnHit;
            health.OnDie += this.OnDie;

            // ActionTable 재설정 (기존 코드 유지)
            ActionTable = new Dictionary<EnemyState, (Action enter, Action exit, Action tick)> {
                { EnemyState.Idle,   (enter: idle.OnEnter,   exit: idle.OnExit,      tick: idle.Tick) },
                { EnemyState.Move,   (enter: move.OnEnter,   exit: move.OnExit,      tick: move.Tick) },
                { EnemyState.Attack, (enter: OnAttackEnter,  exit: OnAttackExit,     tick: OnAttackTick)},
                { EnemyState.Hurt,   (enter: hurt.OnEnter,   exit: hurt.OnExit,      tick: hurt.Tick)},
                { EnemyState.Dead,   (enter: die.OnEnter,    exit: die.OnExit,       tick: die.Tick)},
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
            if (_attackDict.TryGetValue(attackTypeTag, out var attack))
            {
                attack.OnAnimEvent();
            }
        }
    
        public void SetHurtTrigger(bool active) 
        {
            HurtEndTrigger = active;
        }

        public void SetAttackTrigger(bool active) 
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

            EnemyManager.Instance.RemoveEnemy(this);
        }

        #endregion

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

            anim.SetBool(StateAnimHashes[CurrentState], false);
            anim.SetBool(StateAnimHashes[next], true);

            OnExit?.Invoke();
            CurrentState = next;
            (OnEnter, OnExit, OnTick) = ActionTable[next];
            OnEnter?.Invoke();
        }

        public virtual void TakeDamage() 
        {
            if (health.IsDead())
            {
                ColliderActive(false);
                RigidbodyActive(false);
                ChangeState(EnemyState.Dead);
            }
            else
            {
                ChangeState(EnemyState.Hurt);
            }
        }

        protected virtual void OnHit() => ChangeState(EnemyState.Hurt);
        protected virtual void OnDie() => ChangeState(EnemyState.Dead);


        protected virtual void OnDrawGizmos()
        {
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
