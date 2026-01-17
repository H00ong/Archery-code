using System.Collections.Generic;
using Enemy;
using UnityEngine;


[System.Serializable]
public class PatternMove : EnemyMove
{
    private List<Vector3> _patrolPointsPos = new List<Vector3>();
    private int _currentPatrolIndex = 0;

    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);

        _patrolPointsPos.Clear();
        _currentPatrolIndex = 0;

        if (data is PatternMoveData patternData)
        {
            var patrolPoints = patternData.GetPoints(ctx);
            if (patrolPoints != null)
            {
                foreach (var p in patrolPoints)
                {
                    _patrolPointsPos.Add(p.position);
                }
            }
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();

        SetDirection();
    }

    public override void OnExit()
    {
        _ctx.lastPlayerPosition = _player.transform.position;
    }

    public override void Tick()
    {
        base.Tick();
        
        if (_ctx.CurrentState != EnemyState.Move)
            return;

        if (Utils.GetXZDistance(transform.position, _patrolPointsPos[_currentPatrolIndex]) < .5f)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPointsPos.Count;
            SetDirection();
        }

        if (!_ctx.IsBlocked) MoveForward();
    }

    private void SetDirection()
    {
        Vector3 targetPosition = _patrolPointsPos[_currentPatrolIndex];
        Vector3 direction = Utils.GetXZDirectionVector(targetPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
