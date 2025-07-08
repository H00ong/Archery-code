using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] float defaultAttackRange;

    [SerializeField] PlayerManager Player;

    protected override void Start()
    {
        base.Start();

        CurrentState = EnemyState.Idle;
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentState == EnemyState.Idle)
            return;

        Move();
        AttackCheck();
        AttackEndTrigged();
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

        Vector3 dir = Player.transform.position - transform.position;
        dir.y = 0;

        rb.linearVelocity = dir.normalized * defaultMoveSpeed;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    void AttackCheck() 
    {
        if (Vector3.Distance(Player.transform.position, transform.position) < defaultAttackRange 
            && CurrentState == EnemyState.Move) 
        {
            EnemyManager.ChangeState(this, anim, EnemyState.Attack);
        }
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
