using Enemy;
using UnityEngine;


[System.Serializable]
public class FollowMove : EnemyMove
{
    private float _defaultAttackRange;

    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);
        
        if (data is FollowMoveData followData)
        {
            _defaultAttackRange = followData.defaultAttackRange;
        }
        else
        {
            _defaultAttackRange = 2.0f;
        }
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
        if (_ctx.CurrentState != EnemyState.Move) return;

        Vector3 dir = Utils.GetDirectionVector(_player.transform, transform);
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(_player.transform.position, transform.position) < _defaultAttackRange)
        {
            _ctx.lastPlayerPosition = _player.transform.position;
            _ctx.OnModuleComplete();
            return;
        }

        if (!_ctx.IsBlocked) MoveForward();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _defaultAttackRange);
    }
}
    