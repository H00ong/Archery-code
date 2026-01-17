using Enemy;
using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    private float _attackMoveSpeed = 3f;
    
    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);

        if (data is MeleeAttackData meleeData)
        {
            _attackMoveSpeed = meleeData.attackMoveSpeed;
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();

        _ctx.SetAttackMoveTrigger(false);
    }
    
    public override void Tick()
    {
        base.Tick();

        if (_ctx.AttackMoveTrigger) 
            MoveForward();
    }

    protected void MoveForward() 
    {
        transform.position += transform.forward * _attackMoveSpeed * Time.deltaTime;
    }
}
