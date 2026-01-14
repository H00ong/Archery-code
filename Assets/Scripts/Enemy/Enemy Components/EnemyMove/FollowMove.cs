using System;
using System.Xml;
using Enemies;
using Enemy;
using UnityEngine;


[System.Serializable]
public class FollowMove : EnemyMove
{
    [Header("Follow Move Tuning")]
    [SerializeField] float defaultAttackRange;

    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();

        Vector3 dir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public override void Tick()
    {
        UpdateState(EnemyState.Idle);
    }

    protected override void UpdateState(EnemyState state)
    {
        base.UpdateState(state);

        if (_ctx.CurrentState != EnemyState.Move)
            return;

        Vector3 dir = Utils.GetDirectionVector(_player.transform, transform);
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(_player.transform.position, transform.position) < defaultAttackRange)
        {
            _moveTimer = defaultMoveTime;

            _ctx.lastPlayerPosition = _player.transform.position;

            _ctx.ChangeState(EnemyState.Attack);
            return;
        }

        if (!_ctx.IsBlocked) ForwardMove();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
    }
}
    