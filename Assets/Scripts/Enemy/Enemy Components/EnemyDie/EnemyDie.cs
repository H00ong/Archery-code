using Enemies;
using Enemy;
using UnityEngine;

public class EnemyDie : MonoBehaviour, IEnemyBehavior
{
    EnemyController ctx;
    public virtual void Init(EnemyController c) => ctx = c;

    public virtual void OnEnter() 
    {

    }
        
    public virtual void OnExit()
    {
        
    }

    public virtual void Tick()
    {

    }
}
