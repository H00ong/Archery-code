using Game.Enemies.Enum;
using Game.Enemies.Stat;
using System;
using UnityEngine;

public interface IEnemyBehavior
{
    public void Init(EnemyController ctx); 
    public void Tick(); 
    public void OnEnter(); 
    public void OnExit();
}

public interface IAnimationListener 
{
    public void OnAnimEvent();    
}

[System.Serializable]
public class EnemyMove : MonoBehaviour, IEnemyBehavior
{
    protected EnemyController ctx;
    protected EnemyStats stats;
    [Header("Default Tuning")]
    [SerializeField] protected float defaultMoveTime = 4f;
    [SerializeField] protected float defaultMoveSpeed = 1f; // EnemyData
    
    protected PlayerManager player;

    protected float moveTimer;

    public virtual void Init(EnemyController c) 
    {
        ctx = c;

        if (!ctx.debugMode) 
        {
            stats = ctx.stats;
            defaultMoveSpeed = stats.baseStats.moveSpeed;
        }

        moveTimer = defaultMoveTime;
    }

    public virtual void OnEnter() 
    {
        player = ctx.player;
    }

    public virtual void OnExit() 
    {
        ctx.lastPlayerPosition = player.transform.position;
    }

    public virtual void Tick() 
    {
        moveTimer -= Time.deltaTime;

        if (moveTimer < 0)
        {
            moveTimer = defaultMoveTime;
            ctx.ChangeState(EnemyState.Idle);
            return;
        }
    }

    protected void ForwardMove() 
    {
        transform.position += transform.forward * defaultMoveSpeed * Time.deltaTime;
    }
}

[System.Serializable]
public class EnemyAttack : MonoBehaviour, IEnemyBehavior, IAnimationListener
{
    protected EnemyController ctx;
    protected EnemyStats stats;

    protected PlayerManager player;

    public virtual void Init(EnemyController c) 
    {
        ctx = c;

        if (!ctx.debugMode)
        {
            stats = ctx.stats;
        }
        
        ctx.SetAttackTrigger(false);
    } 

    public virtual void OnEnter() 
    {
        player = ctx.player;

        Vector3 dir = Utils.GetDirectionVector(ctx.lastPlayerPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(dir);

        ctx.SetAttackTrigger(false);
    }

    public virtual void Tick() 
    {
        
    }

    public virtual void OnExit() 
    {
        ctx.SetAttackTrigger(false);
    }

    public virtual void OnAnimEvent() { }
}