using Game.Enemies.Enum;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class EnemyHurt : MonoBehaviour, IEnemyBehavior
{
    EnemyController ctx;
    public virtual void Init(EnemyController c) => ctx = c;

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
        if (ctx.HurtTrigger) 
        {
            ctx.ChangeState(EnemyState.Move);
        }
    }
}
