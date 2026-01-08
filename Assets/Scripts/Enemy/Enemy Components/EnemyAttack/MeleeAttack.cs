using Enemies;
using Enemy;
using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    [SerializeField] float attackMoveSpeed;
    public override void Init(EnemyController c)
    {
        base.Init(c);
    }

    public override void OnEnter()
    {
        base.OnEnter();

        _ctx.SetAttackMoveTrigger(false);
    }

    public override void OnExit()
    {
        base.OnExit();

        _ctx.SetAttackMoveTrigger(false);
    }

    public override void Tick()
    {
        base.Tick();

        if (_ctx.AttackMoveTrigger) MoveForward();
    }

    private void MoveForward() 
    {
        transform.position += transform.forward * attackMoveSpeed * Time.deltaTime;
    }
}
