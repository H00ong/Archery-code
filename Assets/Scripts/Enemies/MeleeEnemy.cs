using UnityEngine;

public class MeleeEnemy : Enemy
{
    private bool attackMoveTrigger = false;
    [SerializeField] float defaultAttackRange;
    [SerializeField] float defaultAttackMoveSpeed = 2f;
    Vector3 attackMoveDir = Vector3.zero;

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

        AttackEndTrigged();
        AttackCheck();
        Move();
        AttackMove();
    }

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
            transform.position += attackMoveDir * defaultAttackMoveSpeed * Time.deltaTime;
        }
    }

    void AttackCheck() 
    {
        if (Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange 
            && CurrentState == EnemyState.Move) 
        {
            EnemyManager.ChangeState(this, anim, EnemyState.Attack);
        }
    }

    public void AttackMoveTrigger(bool active) 
    {
        attackMoveTrigger = active;

        if (active)
            attackMoveDir = Utils.GetDirectionVector(player.transform, transform);
        else 
            attackMoveDir = Vector3.zero;
    }

    void AttackEndTrigged() 
    {
        if (attackTrigged) 
        {
            attackTrigged = false;

            EnemyManager.ChangeState(this, anim, EnemyState.Move);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
    }
}
