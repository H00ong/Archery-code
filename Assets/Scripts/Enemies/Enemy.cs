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
    Ranged,
    Boss
}

public enum EnemyName
{
    Slime,
    TurtleShell,
    Skeleton_Minion,
    MeleeGolem,
}

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    public EnemyType EnemyType;
    public EnemyState CurrentState;
    [SerializeField] protected EnemyName enemyName;
    [SerializeField] protected bool debugMode = true;
    protected EnemyData enemyData = null;

    [Space]

    [Header("Default Info")]
    [SerializeField] protected float defaultIdleTime = 1f;
    [SerializeField] protected float defaultMoveTime = 4f;
    [SerializeField] protected float defaultMoveSpeed = 1f; // 나중에 EnemyData에서 읽어와야함.
    [SerializeField] protected float defaultAttackSpeed = 1f;
    
    protected float idleTimer;
    protected float moveTimer;

    protected bool moveTrigger = false;
    protected bool attackTriggered = false;

    protected Animator anim;

    protected PlayerManager player;

    protected virtual void Start()
    {
        // defaultIdleTime = GameManager.Instance.DataManager.GetEnemyData();
        anim = GetComponentInChildren<Animator>();

        if (player == null)
            player = FindAnyObjectByType<PlayerManager>();

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

    public void AnimationTrigger() => attackTriggered = true;
}
