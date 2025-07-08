using UnityEngine;

public enum EnemyState
{
    Idle,
    Attack,
    Move,
    Dead
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
}

public abstract class Enemy : MonoBehaviour
{
    public EnemyType EnemyType;
    public EnemyState CurrentState;
    [SerializeField] protected EnemyName enemyName;

    [Header("Default Info")]
    [SerializeField] protected float defaultIdleTime = 1f;
    [SerializeField] protected float defaultMoveTime = 4f;
    [SerializeField] protected float defaultMoveSpeed = 1f; // 나중에 EnemyData에서 읽어와야함.
    
    protected float idleTimer;
    protected float moveTimer;

    protected bool moveTrigger = false;
    protected bool attackTrigged = false;

    protected Animator anim;
    protected Rigidbody rb;

    protected virtual void Start()
    {
        // defaultIdleTime = GameManager.Instance.DataManager.GetEnemyData();
        anim = GetComponentInChildren<Animator>();
        rb   = GetComponent<Rigidbody>();

        defaultMoveSpeed = DataManager.Instance.GetEnemyData(enemyName.ToString()).moveSpeed;
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

    public void AnimationTrigger() => attackTrigged = true;
}
