using Enemy;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class EnemyHurt : MonoBehaviour, IEnemyBehavior
{
    EnemyController _ctx;
    public virtual void Init(EnemyController ctx, BaseModuleData data = null) => this._ctx = ctx;

    public virtual void OnEnter()
    {
        _ctx.SetHurtEndTrigger(false);
        
        Vector3 dir = Utils.GetXZDirectionVector(_ctx.player.transform.position, _ctx.transform.position);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public virtual void OnExit()
    {
        _ctx.SetHurtEndTrigger(false);
    }

    public virtual void Tick()
    {
        if (_ctx.HurtEndTrigger) 
        {
            _ctx.OnModuleComplete();
        }
    }
}
