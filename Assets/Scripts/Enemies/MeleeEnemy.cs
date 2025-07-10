using UnityEngine;

public class MeleeEnemy : Enemy
{
    private bool attackMoveTrigger = false;
    [SerializeField] float defaultAttackRange;
    [SerializeField] float defaultAttackMoveSpeed = 2f;

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

        CurrentState = EnemyState.Idle;
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentState == EnemyState.Idle)
            return;

        AttackEndTriggered();
        AttackCheck();
        Move();
        AttackMove();
    }

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

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
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
        if (Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange 
            && CurrentState == EnemyState.Move) 
        {
            transform.rotation = Quaternion.LookRotation(Utils.GetDirectionVector(player.transform, transform), Vector3.up);
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

    #endregion

    #region Animation Events
    public void AttackMoveTrigger(bool active) => attackMoveTrigger = active;
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
    }
}
