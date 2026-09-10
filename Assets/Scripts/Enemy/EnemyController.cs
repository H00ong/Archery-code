using System;
using System.Collections;
using System.Collections.Generic;
using Effects;
using Managers;
using Map;
using Players;
using UnityEngine;


namespace Enemy
{
    public class EnemyController : MonoBehaviour, IStunReceiver, ICrowdControlImmune
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
        [SerializeField, RegistryKey("enemyNames")] public string enemyName;
        [SerializeField] public EnemyTag enemyTags;
        private EnemyIdentity _identity;
        private EnemyStat _stat;
        
        public EnemyStat stat => _stat;
        public EnemyState CurrentState { get; private set; }
        public bool IsBoss => EnemyTagUtil.Has(enemyTags, EnemyTag.Boss);

        [Header("Modules")]
        private EnemyBrain brain;
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
        public EnemyHealthBar healthBar;

        [Header("Idle Tuning")]
        [SerializeField] private float defaultIdleTime = 2f;

        [Header("Default Tuning")]
        [SerializeField] private float defaultAttackSpeed;


        [HideInInspector] public Vector3 lastPlayerPosition;
        [HideInInspector] public PlayerController player;

        public bool AttackMoveTrigger { get; set; }
        public bool HurtEndTrigger { get; set; }
        public bool AttackEndTrigger { get; set; }
        public bool HasMultiAttackModules => _attacks.Count > 1;

        /// <summary> Lightning 등 스턴 상태 여부. EnemyHurt에서 이 부를 확인해 Hurt 상태를 유지한다. </summary>
        public bool IsStunned { get; private set; }
    
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
            if (!rigidBody.isKinematic)
            {
                rigidBody.linearVelocity = Vector3.zero;
            }
            
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

            // 풀링된 개체의 이전 생에서 남은 상태(예: 사망 직전 Lightning에 스턴되어 IsStunned=true인 채 가 버린 경우)를 초기화한다.
            IsStunned = false;

            // EnemyIdentity가 제공되면 tag와 visual 주입
            if (identity != null)
            {
                SetIdentity(identity);
            }

            player = PlayerController.Instance;

            RigidbodyActive(true);
            ColliderActive(true);
            SetStat();
            InitModule(_identity);

            anim.SetFloat(AnimHashes.AttackSpeed, defaultAttackSpeed);
            InitState(EnemyState.Idle);
        }
        
        private void SetIdentity(EnemyIdentity identity)
        {
            _identity = identity;
            enemyTags = identity.Tag;

            brain = identity.brain;
            
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
            if (!healthBar) healthBar = GetComponentInChildren<EnemyHealthBar>();
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

        private void InitMoveModule(EnemyIdentity identity)
        {
            move = EnemyBehaviorFactory.CreateMoveModules(this, enemyTags, identity?.moveModule, _modules);
        }

        private void InitAttackModule(EnemyIdentity identity)
        {
            EnemyBehaviorFactory.CreateAttackModules(this, enemyTags, identity?.attackModules, _modules, _attacks);
        }

        private void ClearAction()
        {
            OnEnter = null;
            OnTick = null;
            OnExit = null;

            health.OnDie -= this.OnDie;
            health.OnHit -= this.OnHit;
        }

        private void InitModule(EnemyIdentity identity)
        {
            ClearAction();

            enemyReference.Init();

            _attacks.Clear();
            _modules.Clear();

            InitMoveModule(identity);
            InitAttackModule(identity);

            idle = gameObject.GetOrAddComponent<EnemyIdle>();
            idle.SetIdleTime(defaultIdleTime);
            idle.Init(this);
            
            die = gameObject.GetOrAddComponent<EnemyDie>();   die.Init(this);
            hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);
            health = gameObject.GetOrAddComponent<Health>();  health.InitializeHealth(stat.MaxHP);

            healthBar?.Initialize(health, transform);

            health.OnHit += this.OnHit;
            health.OnDie += this.OnDie;

            // _attacks/move가 비어있으면(태그 설정 누락 등) 여기서 예외가 나 InitState 이전에 초기화가 중단되고,
            // 그 결과 OnEnter/OnTick/OnExit이 계속 null로 남아 어떤 상태로도 진입하지 못한 채 완전히 멈춘 개체가 된다.
            // 그런 경우에도 나머지 상태(Idle/Hurt/Dead)는 정상 동작하도록 방어적으로 구성한다.
            ActionTable = new Dictionary<EnemyState, (Action enter, Action exit, Action tick)>
            {
                { EnemyState.Idle, (enter: idle.OnEnter, exit: idle.OnExit, tick: idle.Tick) },
                { EnemyState.Hurt, (enter: hurt.OnEnter, exit: hurt.OnExit, tick: hurt.Tick) },
                { EnemyState.Dead, (enter: die.OnEnter, exit: die.OnExit, tick: die.Tick) },
            };

            if (move != null)
                ActionTable[EnemyState.Move] = (enter: move.OnEnter, exit: move.OnExit, tick: move.Tick);
            else
                Debug.LogWarning($"[EnemyController] '{enemyName}': move module이 없습니다 (enemyTags에 이동 태그 누락 가능성). Move 상태 진입 시 아무 동작도 하지 않습니다.", this);

            if (_attacks.Count > 0)
                ActionTable[EnemyState.Attack] = (enter: _attacks[0].OnEnter, exit: _attacks[0].OnExit, tick: _attacks[0].Tick);
            else
                Debug.LogWarning($"[EnemyController] '{enemyName}': attack module이 없습니다 (enemyTags에 공격 태그 누락 가능성). Attack 상태 진입 시 아무 동작도 하지 않습니다.", this);
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
            if (!ActionTable.TryGetValue(initState, out var entry))
            {
                Debug.LogWarning($"[EnemyController] '{enemyName}': ActionTable에 {initState} 상태가 없어 Idle로 대체합니다.", this);
                initState = EnemyState.Idle;
                entry = ActionTable[initState];
            }

            CurrentState = initState;
            (OnEnter, OnExit, OnTick) = entry;

            // 재사용(풀링)된 개체는 이전 생애의 Animator bool(Dead 등)이 그대로 남아있을 수 있으므로 전부 초기화한다.
            foreach (var hash in StateAnimHashes.Values)
                SafeSetBool(hash, false);
            SafeSetBool(StateAnimHashes[initState], true);

            OnEnter?.Invoke();
        }

        public void ChangeState(EnemyState next)
        {
            if (CurrentState == next) return;
            if (CurrentState == EnemyState.Dead) return;

            if (!ActionTable.TryGetValue(next, out var entry))
            {
                Debug.LogWarning($"[EnemyController] '{enemyName}': ActionTable에 {next} 상태가 없어 Idle로 대체합니다.", this);
                next = EnemyState.Idle;
                entry = ActionTable[next];
            }

            OnExit?.Invoke();
            SafeSetBool(StateAnimHashes[CurrentState], false);

            CurrentState = next;

            if (CurrentState == EnemyState.Attack)
            {
                ChooseAttackModule();
                entry = ActionTable[next];
            }

            (OnEnter, OnExit, OnTick) = entry;
            OnEnter?.Invoke();
            SafeSetBool(StateAnimHashes[next], true);
        }
        
        private void ChooseAttackModule()
        {
            if (_attacks.Count <= 1) return;

            var currentAttackIndex = 0;//UnityEngine.Random.Range(0, _attacks.Count);
            var selectedAttack = _attacks[currentAttackIndex];
            
            ActionTable[EnemyState.Attack] = (selectedAttack.OnEnter, selectedAttack.OnExit, selectedAttack.Tick);
        }

        // 이 Animator가 실제로 가진 Bool 파라미터 해시값 캐시 (Boss처럼 일부 상태 bool이 없는 컨트롤러 대응)
        private HashSet<int> _boolParamHashes;

        private bool HasBoolParam(int hash)
        {
            if (_boolParamHashes == null)
            {
                _boolParamHashes = new HashSet<int>();
                foreach (var p in anim.parameters)
                {
                    if (p.type == AnimatorControllerParameterType.Bool)
                        _boolParamHashes.Add(p.nameHash);
                }
            }

            return _boolParamHashes.Contains(hash);
        }

        private void SafeSetBool(int hash, bool value)
        {
            if (HasBoolParam(hash))
                anim.SetBool(hash, value);
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

        // ================================================================
        //  IStunReceiver 구현 (Lightning 등)
        // ================================================================

        /// <summary> Lightning 등 스턴 시작 — Hurt 상태로 강제 진입시키고 자동 종료를 지연시킨다. </summary>
        public void BeginStun()
        {
            // Boss는 Ice/Lightning 면역이라 Health가 애초에 Apply를 호출하지 않지만, 방어적으로 한 번 더 막는다.
            if (IsBoss) return;
            if (CurrentState == EnemyState.Dead) return;

            IsStunned = true;
            SetAttackEndTrigger(false);
            SetHurtEndTrigger(false);

            if (rigidBody != null)
                rigidBody.linearVelocity = Vector3.zero;

            if (CurrentState != EnemyState.Hurt)
                ChangeState(EnemyState.Hurt);
        }

        /// <summary> 스턴 종료 — Hurt 애니메이션 이벤트 대기 없이 다음 행동으로 복귀한다. </summary>
        public void EndStun()
        {
            if (!IsStunned) return;

            IsStunned = false;

            if (CurrentState == EnemyState.Hurt)
                OnModuleComplete();
        }

        public void ReturnImmediately()
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }

        /// <summary> Boss는 Ice/Lightning(군중제어형 상태이상)에 완전히 면역이다. </summary>
        public bool IsImmuneTo(EffectType type) => IsBoss && Utils.HasEffectType(type, EffectType.Ice | EffectType.Lightning);

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                rigidBody.linearVelocity = Vector3.zero;

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