using UnityEngine;

public enum EnemyState
{
    Idle,
    Attack,
    Move,
    Dead,
    Hurt
}

public enum EnemyType
{
    Melee,
    Ranged_Pattern,
    Ranged_Follow,
    Ranged_Random,
    Boss_Follow,
    Boss_Ranged
}

public enum EnemyName
{
    Slime,
    TurtleShell,
    Skeleton_Minion,
    Skeleton_Warrior,
    Skeleton_Rogue,
    MeleeGolem,
    Mushroom,
    Cactus,
    Box,
    Stingray,
    Bird,
}

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    public EnemyType EnemyType;
    [SerializeField] protected EnemyName enemyName;
    [SerializeField] protected bool debugMode = true;
    protected EnemyData enemyData = null;
    public EnemyState CurrentState { get; set; }

    [Space]
    [Header("Default Info")]
    [SerializeField] protected float defaultIdleTime = 1f;
    [SerializeField] protected float defaultMoveTime = 4f;
    [SerializeField] protected float defaultMoveSpeed = 1f; // 나중에 EnemyData에서 읽어와야함.
    [SerializeField] protected float defaultAttackSpeed = 1f;
    protected float idleTimer;
    protected float moveTimer;

    #region Components
    protected Animator anim;
    public EnemyHealth enemyHealth { get; set; }
    #endregion

    protected bool moveTrigger = false;
    protected bool attackTriggered = false;
    protected bool hurtTriggered = false;

    protected PlayerManager player;
    const int enemyLayer = 3; // Enemy Layer


    protected virtual void Awake()
    {
        GameObject go = GetComponentInChildren<Collider>().gameObject;
        go.gameObject.layer = enemyLayer;
        gameObject.tag = "Enemy";
    }

    protected virtual void Start()
    {
        if (!debugMode)
        {
            enemyData = GameManager.Instance.DataManager.GetEnemyData(enemyName.ToString());

            if (enemyData != null)
            {
                defaultIdleTime = enemyData.idleTime;
                defaultMoveTime = enemyData.moveTime;
                defaultMoveSpeed = enemyData.moveSpeed;
                defaultAttackSpeed = enemyData.attackSpeed;

                anim.SetFloat("AttackSpeed", defaultAttackSpeed);
            }
        }

        anim.SetFloat("AttackSpeed", defaultAttackSpeed);
    }


    protected virtual void OnEnable()
    {
        DefaultSetting();
    }

    protected virtual void DefaultSetting()
    {
        anim = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        enemyHealth.InitializeHealth();

        if (player == null)
            player = FindAnyObjectByType<PlayerManager>();

        ColliderActive(true); // Enable collider on spawn

        idleTimer = defaultIdleTime;
    }

    protected virtual void Update()
    {
        if (CurrentState == EnemyState.Idle)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer < 0)
            {
                idleTimer = defaultIdleTime;
                EnemyManager.ChangeState(this, anim, EnemyState.Move);
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

    }

    public virtual int GetAttackDamage() 
    {
        return -1;
    }

    public virtual void GetHit(float _damage)
    {
        if (CurrentState == EnemyState.Dead)
            return;

        enemyHealth.TakeDamage(_damage);

        if (enemyHealth.IsDead())
        {
            ColliderActive(false); // Disable collider on death
            RigidbodyActive(false); // Disable rigidbody on death
            EnemyManager.enemies.Remove(this);
            EnemyManager.ChangeState(this, anim, EnemyState.Dead);
        }
        else
        {
            hurtTriggered = false; // Reset hurt trigger
            EnemyManager.ChangeState(this, anim, EnemyState.Hurt);
        }
    }

    protected void ColliderActive(bool active)
    {
        Collider cd = GetComponentInChildren<Collider>();

        if (cd != null)
        {
            cd.enabled = active;
        }
    }

    protected void RigidbodyActive(bool active)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = !active;
        }
    }

    #region Animation Events
    public void AttackTrigger() => attackTriggered = true;
    public void HurtTrigger() => hurtTriggered = true;
    public virtual void Die()
    {

    }
    #endregion
}
