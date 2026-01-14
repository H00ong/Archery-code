using NUnit.Framework;
using System.Collections.Generic;
using Enemies;
using Enemy;
using UltimateProceduralPrimitivesFREE;
using UnityEngine;

[System.Serializable]
public class PatternMove : EnemyMove
{
    [Header("Patrol")]
    [SerializeField] Transform[] patrolPoints = null;
    List<Vector3> patrolPointsPos = new List<Vector3>();
    int currentPatrolIndex = 0;

    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);

        currentPatrolIndex = 0;

        patrolPointsPos.Clear();

        foreach (var p in patrolPoints) { patrolPointsPos.Add(p.position); }
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
        UpdateState(EnemyState.Attack);
    }

    protected override void UpdateState(EnemyState state)
    {
        base.UpdateState(state);

        if (_ctx.CurrentState != EnemyState.Move)
            return;

        if (Utils.GetXZDistance(transform.position, patrolPointsPos[currentPatrolIndex]) < .5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPointsPos.Count;
            SetDirection();
        }

        if (!_ctx.IsBlocked) ForwardMove();
    }

    private void SetDirection()
    {
        Vector3 targetPosition = patrolPointsPos[currentPatrolIndex];
        Vector3 direction = Utils.GetXZDirectionVector(targetPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
