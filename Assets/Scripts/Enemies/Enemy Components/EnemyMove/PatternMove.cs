using Game.Enemies.Enum;
using UltimateProceduralPrimitivesFREE;
using UnityEngine;

[System.Serializable]
public class PatternMove : EnemyMove
{
    [Header("Patrol")]
    [SerializeField] Transform patrolPointsParent;
    public Transform[] patrolPoints = null;
    int currentPatrolIndex = 0;

    public override void Init(EnemyController c)
    {
        base.Init(c);

        currentPatrolIndex = 0;
        foreach (var p in patrolPoints) 
        {
            p.SetParent(patrolPointsParent);
        }
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
        base.Tick();

        if (ctx.currentState != EnemyState.Move)
            return;

        if (Utils.GetXZDistance(transform.position, patrolPoints[currentPatrolIndex].position) < .5f)
        {
            // 현재 목표 지점에 도달하면 index 하나 증가
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            SetDirection();
        }

        if (!ctx.isBlocked) ForwardMove();
    }

    private void SetDirection()
    {
        Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
        Vector3 direction = Utils.GetDirectionVector(targetPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
