using UnityEngine;

public class MeleeEnemy : Enemy
{
    private bool attackMoveTrigger = false;
    [SerializeField] int defaultMeleeDamage = 1;
    [SerializeField] float defaultAttackRange;
    [SerializeField] float defaultAttackMoveSpeed = 2f;

    #region Initialization

    protected override void Start()
    {
        base.Start();

        if (!debugMode)
        {
            if (enemyData != null)
            {
                defaultAttackRange = enemyData.attackRange;
                defaultAttackMoveSpeed = enemyData.attackMoveSpeed;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void DefaultSetting()
    {
        base.DefaultSetting();

        EnemyManager.ChangeState(this, anim, EnemyState.Idle);
    }

    #endregion

    protected override void Update()
    {
        base.Update();

        if (CurrentState == EnemyState.Dead)
            return;

        if (CurrentState == EnemyState.Idle)
            return;

        HurtEndTriggered();
        AttackEndTriggered();
        AttackCheck();
        Move();
        AttackMove();
    }

    #region Get Methods
    public override int GetAttackDamage()
    {
        return defaultMeleeDamage;
    }
    #endregion

    #region Melee Enemy Actions



    void Move()
    {
        if (CurrentState != EnemyState.Move)
            return;

        moveTimer -= Time.deltaTime;

        if (moveTimer < 0)
        {
            moveTimer = defaultMoveTime;
            EnemyManager.ChangeState(this, anim, EnemyState.Idle);
            return;
        }

        Vector3 dir = Utils.GetDirectionVector(player.transform, transform);
        transform.position += dir * defaultMoveSpeed * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    void AttackMove() 
    {
        if (attackMoveTrigger) 
        {
            transform.position += transform.forward * defaultAttackMoveSpeed * Time.deltaTime;
        }
    }

    void AttackCheck() 
    {
        bool distanceCondition = Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange;
        bool stateCondition = CurrentState == EnemyState.Move;
        if (distanceCondition && stateCondition)
        {
            transform.rotation = Quaternion.LookRotation(Utils.GetDirectionVector(player.transform, transform));
            EnemyManager.ChangeState(this, anim, EnemyState.Attack);
        }
    }

    void AttackEndTriggered() 
    {
        if (attackTriggered) 
        {
            attackTriggered = false;

            EnemyManager.ChangeState(this, anim, EnemyState.Move);
        }
    }

    void HurtEndTriggered()
    {
        if (hurtTriggered)
        {
            hurtTriggered = false;
            EnemyManager.ChangeState(this, anim, EnemyState.Move);
        }
    }

    #endregion

    #region Animation Events
    public void AttackMoveTrigger(bool active) => attackMoveTrigger = active;
    public override void Die() 
    {
        base.Die();
        Destroy(gameObject, .5f);
    }
    #endregion

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
    }
}
