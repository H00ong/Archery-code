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
        // 스턴(Lightning 등) 중이마음 Hurt 종료를 무시하여 Hurt 상태를 유지한다.
        if (_ctx.IsStunned) return;

        if (_ctx.HurtEndTrigger) 
        {
            _ctx.OnModuleComplete();
        }
    }
}
