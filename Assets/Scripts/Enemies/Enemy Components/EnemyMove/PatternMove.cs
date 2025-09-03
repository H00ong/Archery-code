using Game.Enemies.Enum;
using NUnit.Framework;
using System.Collections.Generic;
using UltimateProceduralPrimitivesFREE;
using UnityEngine;

[System.Serializable]
public class PatternMove : EnemyMove
{
    [Header("Patrol")]
    [SerializeField] Transform[] patrolPoints = null;
    List<Vector3> patrolPointsPos = new List<Vector3>();
    int currentPatrolIndex = 0;

    public override void Init(EnemyController c)
    {
        base.Init(c);

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
        ctx.lastPlayerPosition = player.transform.position;
    }

    public override void Tick()
    {
        UpdateState(EnemyState.Attack);
    }

    protected override void UpdateState(EnemyState _state)
    {
        base.UpdateState(_state);

        if (ctx.CurrentState != EnemyState.Move)
            return;

        if (Utils.GetXZDistance(transform.position, patrolPointsPos[currentPatrolIndex]) < .5f)
        {
            // 현재 목표 지점에 도달하면 index 하나 증가
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPointsPos.Count;
            SetDirection();
        }

        if (!ctx.IsBlocked) ForwardMove();
    }

    private void SetDirection()
    {
        Vector3 targetPosition = patrolPointsPos[currentPatrolIndex];
        Vector3 direction = Utils.GetDirectionVector(targetPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
