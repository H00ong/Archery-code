using Enemies;
using Enemy;
using UnityEngine;

public class EnemyDie : MonoBehaviour, IEnemyBehavior
{
    private EnemyController _ctx;
    public virtual void Init(EnemyController c) => _ctx = c;

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
