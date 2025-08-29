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
        base.Tick();

        if (ctx.currentState != EnemyState.Move)
            return;

        if (Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange)
        {
            moveTimer = defaultMoveTime;
            ctx.ChangeState(EnemyState.Attack);
            return;
        }

        Vector3 dir = Utils.GetDirectionVector(player.transform, transform);
        transform.rotation = Quaternion.LookRotation(dir);

        if (!ctx.isBlocked) ForwardMove();

    }
}
