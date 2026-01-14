using Enemies;
using Enemy;
using UnityEngine;

[System.Serializable]
public class EnemyIdle : MonoBehaviour, IEnemyBehavior
{
    [Header("Idle Tuning")]
    [SerializeField] float defaultIdleTime = 10f;
    float idleTimer = 0f;
    
    EnemyController ctx;
    public void Init(EnemyController ctx, BaseModuleData data = null) 
    {
        this.ctx = ctx;
        idleTimer = defaultIdleTime;
    }

    public void OnEnter()
    {
        idleTimer = defaultIdleTime;
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer < 0)
        {
            idleTimer = defaultIdleTime;
            ctx.ChangeState(EnemyState.Move);
        }
    }
}