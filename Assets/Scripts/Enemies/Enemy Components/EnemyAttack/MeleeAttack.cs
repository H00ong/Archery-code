using Game.Enemies.Enum;
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

        ctx.SetAttackMoveTrigger(false);
    }

    public override void OnExit()
    {
        base.OnExit();

        ctx.SetAttackMoveTrigger(false);
    }

    public override void Tick()
    {
        base.Tick(); // empty

        if (ctx.attackMoveTrigger) MoveForward();
    }

    private void MoveForward() 
    {
        transform.position += transform.forward * attackMoveSpeed * Time.deltaTime;
    }
}
