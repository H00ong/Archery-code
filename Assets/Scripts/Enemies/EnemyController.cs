using Game.Enemies;
using Game.Enemies.Enum;
using Game.Enemies.Stat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Debug")]
    public bool debugMode = true;

    [Header("Identity")]
    [SerializeField] EnemyName enemyName;
    [SerializeField] public EnemyTag enemyTags;
    EnemyData data;
    public EnemyStats stats;
    public EnemyState CurrentState { get; protected set; }

    [Header("Required Managers")]
    public DataManager dataManager = null;
    public PoolManager poolManager = null;
    public PlayerManager player = null;

    [Header("Modules")]
    [SerializeField] protected EnemyIdle idle;
    [SerializeField] protected EnemyMove move;
    [SerializeField] protected EnemyHurt hurt;
    [SerializeField] protected EnemyDie die;
    public List<EnemyAttack> attackList = new List<EnemyAttack>();
    public Dictionary<EnemyTag, EnemyAttack> attackDict = new Dictionary<EnemyTag, EnemyAttack>();

    [Header("Components")]
    public EnemyHealth health;
    public Animator anim;
    public Rigidbody rb;
    public Collider cd;

    [Header("Default Tuning")]
    [SerializeField] protected float defaultAttackSpeed; // attack anim speed;
    [SerializeField] protected int defaultAtk;

    [Header("Collision")]
    [SerializeField] protected Transform blockCheck;
    [SerializeField] protected float sphereCastRadius = 0f;
    [SerializeField] protected float castDistance = .5f;
    [SerializeField] protected LayerMask obstacleLayer;

    protected Action onEnter, onExit, onTick;
    protected Action attackOnEnter, attackOnExit, attackOnTick;

    protected Dictionary<EnemyState, (Action enter, Action exit, Action tick)> actionTable;

    [HideInInspector] public Vector3 lastPlayerPosition;

    public bool IsBlocked { get; protected set; } = false;
    public bool AttackTrigger { get; protected set; } = false;
    public bool AttackMoveTrigger { get; protected set; } = false;
    public bool HurtTrigger { get; protected set; } = false;

    protected readonly Dictionary<EnemyState, int> animBool = new()
    {
        { EnemyState.Idle,   Animator.StringToHash("Idle")   },
        { EnemyState.Hurt,   Animator.StringToHash("Hurt")   },
        { EnemyState.Attack, Animator.StringToHash("Attack") },
        { EnemyState.Dead,   Animator.StringToHash("Dead")   },
        { EnemyState.Move,   Animator.StringToHash("Move")   },
    };

    #region Unity Cycle


    private void BlockCheck()
    {
        Vector3 origin = blockCheck.position;
        float radius = sphereCastRadius;
        Vector3 dir = transform.forward;
        float checkDistance = castDistance;
        LayerMask mask = obstacleLayer;

        if (Physics.SphereCast(origin,
                                radius,
                                dir,
                                out RaycastHit hit,
                                checkDistance,
                                mask))
        {
            if (move is RandomMove randomMove)
            {
                // hit.normal을 이용해 정확한 반사
                randomMove.PickReflectDirection(transform.forward, hit.normal);
            }
            else
                IsBlocked = true;
        }
        else
        {
            IsBlocked = false;
        }
    }

    protected virtual void Update()
    {
        BlockCheck();

        if (AttackTrigger)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        onTick?.Invoke();
    }

    protected virtual void OnDisable()
    {
        ClearAction();
    }
    #endregion

    #region Initialization
    public void InitializeEnemy(MapData _mapData, int _stageIndex)
    {
        SetupManager();
        CacheComponent();
        RigidbodyActive(true);
        ColliderActive(true); // Enable collider on spawn
        ApplyEnemyData(_mapData, _stageIndex);
        AssignPlayerManager();
        InitModule();

        anim.SetFloat(AnimHashes.AttackSpeed, defaultAttackSpeed);
        InitState(EnemyState.Idle);

    }

    private void SetupManager()
    {
        if (!dataManager) dataManager = FindAnyObjectByType<DataManager>();
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();
    }

    private void CacheComponent()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (cd == null) cd = GetComponentInChildren<Collider>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    protected virtual void ApplyEnemyData(MapData _mapData, int _stageIndex)
    {
        if (!debugMode)
        {
            data = dataManager.GetEnemyData(enemyName, enemyTags);
            if (data == null)
            {
                Debug.LogError("Enemy data is null");
                return;
            }

            MapData mapData = MapManager.currentMapData;
            if (mapData == null)
            {
                Debug.LogError("Map data is null");
                return;
            }

            stats = EnemyStatUtil.GetEnemyStats(data, enemyTags, _mapData, _stageIndex);

            var baseStats = stats.baseStats;
            health.InitializeHealth(baseStats.hp);
            defaultAtk = baseStats.atk;
        }
    }

    private void AssignPlayerManager() => player = FindAnyObjectByType<PlayerManager>();

    protected void ColliderActive(bool active) => cd.enabled = active;

    protected void RigidbodyActive(bool active)
    {
        if (rb != null)
        {
            rb.isKinematic = !active;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    protected virtual void InitMoveModule()
    {
        foreach (var (tag, type) in EnemyBehaviorFactories.MoveFactory)
        {
            // EnemyController가 가지고 있는 EnemyTag와 팩토리 태그가 일치하는지 확인
            if (EnemyTagUtil.Has(enemyTags, tag))  // enemyTags는 EnemyController 필드 가정
            {
                move = (EnemyMove)gameObject.GetOrAddComponent(type); // ← Type 오버로드 사용
                move.Init(this);
                break;
            }
        }

        // 매칭되는 태그가 없으면 기본 Move 설정 (예: IdleMove)
        if (move == null)
        {
            Debug.LogError("No Enemy Move Tag");
        }
    }

    protected void InitAttackModule()
    {
        EnemyBehaviorFactories.CreateAttackModules(this, enemyTags, attackList, attackDict);

        foreach (var atk in attackList) 
        {
            atk.Init(this);

            attackOnEnter += atk.OnEnter;
            attackOnTick += atk.Tick;
            attackOnExit += atk.OnExit;
        }
    }

    protected void ClearAction() 
    {
        onEnter = null;
        onTick = null;
        onExit = null;

        attackOnEnter = null;
        attackOnExit = null;
        attackOnTick = null;
    }

    protected virtual void InitModule()
    {
        ClearAction();
        InitMoveModule();
        InitAttackModule();

        idle = gameObject.GetOrAddComponent<EnemyIdle>(); idle.Init(this);
        die = gameObject.GetOrAddComponent<EnemyDie>(); die.Init(this);
        hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);

        actionTable = new() {
      { EnemyState.Idle,   (enter: idle.OnEnter,   exit: idle.OnExit,      tick: idle.Tick) },
      { EnemyState.Move,   (enter: move.OnEnter,   exit: move.OnExit,      tick: move.Tick)  },
      { EnemyState.Attack, (enter: attackOnEnter,  exit: attackOnExit,     tick: attackOnTick)},
      { EnemyState.Hurt,   (enter: hurt.OnEnter,   exit : hurt.OnExit,     tick: hurt.Tick)},
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

    public void Ability(EnemyTag tag) 
    {
        EnemyAttack attack = attackDict[tag];
        attack.OnAnimEvent();
    }
    
    public void SetHurtTrigger(bool active) 
    {
        HurtTrigger = active;
    }

    public void SetAttackTrigger(bool active) 
    {
        AttackTrigger = active;
    }

    public void SetAttackMoveTrigger(bool active) 
    {
        AttackMoveTrigger = active;
    }

    public void Die() 
    {
        StartCoroutine(ReturnCoroutine());
    }

    IEnumerator ReturnCoroutine() 
    {
        yield return new WaitForSeconds(.5f);

        poolManager.ReturnObject(gameObject);
    }

    #endregion

    protected void InitState(EnemyState initState) 
    {
        CurrentState = initState;
        (onEnter, onExit, onTick) = actionTable[initState];
        onEnter?.Invoke();
    }

    public virtual void ChangeState(EnemyState next)
    {
        if (CurrentState == next) return;

        if (CurrentState == EnemyState.Dead) return;

        anim.SetBool(animBool[CurrentState], false);
        anim.SetBool(animBool[next], true);

        onExit?.Invoke();
        CurrentState = next;
        (onEnter, onExit, onTick) = actionTable[next];
        onEnter?.Invoke();
    }

    public virtual void GetHit(int _damage) 
    {
        if (CurrentState == EnemyState.Dead)
            return;

        health.TakeDamage(_damage);

        if (health.IsDead())
        {
            EnemyManager.RemoveEnemy(this);
            ColliderActive(false); // Disable collider on death
            RigidbodyActive(false); // Disable rigidbody on death            
            ChangeState(EnemyState.Dead);
        }
        else
        {
            ChangeState(EnemyState.Hurt);
        }
    }

    [ContextMenu("Apply LayerMask")]
    protected void TagLayerSetup()
    {
        GameObject go = GetComponentInChildren<Collider>().gameObject;
        go.gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    [ContextMenu("Apply Tag")]
    protected void ApplyTag()
    {
        gameObject.tag = "Enemy";
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(blockCheck.position, sphereCastRadius);
    }

#if UNITY_EDITOR
    // 프리팹 편집 시 자동 보정
    protected virtual void OnValidate()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!health) health = GetComponent<EnemyHealth>();
        if (!cd) cd = GetComponentInChildren<Collider>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!dataManager) dataManager = FindAnyObjectByType<DataManager>();
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();
        if (!player) player = FindAnyObjectByType<PlayerManager>();

        if (!idle) GetComponent<EnemyIdle>();
        if(!die) GetComponent<EnemyDie>();
        if(!hurt) GetComponent<EnemyHurt>();
        if(!move) GetComponent<EnemyMove>();

        TagLayerSetup();
        ApplyTag();
    }
#endif
}

// 전략 추상
