using Game.Enemies.Enum;
using System;
using System.Xml;
using UnityEngine;


[System.Serializable]
public class FollowMove : EnemyMove
{
    [Header("Follow Move Tuning")]
    [SerializeField] float defaultAttackRange;

    public override void Init(EnemyController c)
    {
        base.Init(c);
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();

        Vector3 dir = Utils.GetDirectionVector(ctx.lastPlayerPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public override void Tick()
    {
        UpdateState(EnemyState.Idle);
    }

    protected override void UpdateState(EnemyState _state)
    {
        base.UpdateState(_state);

        if (ctx.CurrentState != EnemyState.Move)
            return;

        Vector3 dir = Utils.GetDirectionVector(player.transform, transform);
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange)
        {
            moveTimer = defaultMoveTime;

            ctx.lastPlayerPosition = player.transform.position;

            ctx.ChangeState(EnemyState.Attack);
            return;
        }

        if (!ctx.IsBlocked) ForwardMove();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
    }
}
    