using Enemy;
using UnityEngine;

public class EnemyDie : MonoBehaviour, IEnemyBehavior
{
    private EnemyController _ctx;
    public virtual void Init(EnemyController ctx, BaseModuleData data = null) => _ctx = ctx;

    public virtual void OnEnter() 
    {
        _ctx.ColliderActive(false);
        _ctx.RigidbodyActive(false);
    }
        
    public virtual void OnExit()
    {
        
    }

    public virtual void Tick()
    {

    }
}
