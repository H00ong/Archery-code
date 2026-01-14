using Enemies;
using Enemy;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class EnemyHurt : MonoBehaviour, IEnemyBehavior
{
    EnemyController ctx;
    public virtual void Init(EnemyController ctx, BaseModuleData data = null) => this.ctx = ctx;

    public virtual void OnEnter()
    {
        ctx.SetHurtTrigger(false);
    }

    public virtual void OnExit()
    {
        ctx.SetHurtTrigger(false);
    }

    public virtual void Tick()
    {
        if (ctx.HurtEndTrigger) 
        {
            ctx.ChangeState(EnemyState.Move);
        }
    }
}
