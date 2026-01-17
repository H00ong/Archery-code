using Enemy;
using UnityEngine;

[System.Serializable]
public class EnemyIdle : MonoBehaviour, IEnemyBehavior
{
    private float _defaultIdleTime;
    private float _idleTimer;
    
    private EnemyController _ctx;
    
    public void SetIdleTime(float idleTime)
    {
        _defaultIdleTime = idleTime;
    }
    
    public void Init(EnemyController ctx, BaseModuleData data = null) 
    {
        _ctx = ctx;
        _idleTimer = _defaultIdleTime;
    }

    public void OnEnter()
    {
        _idleTimer = _defaultIdleTime;
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
        _idleTimer -= Time.deltaTime;

        if (_idleTimer < 0)
        {
            _idleTimer = _defaultIdleTime;
            _ctx.ChangeState(EnemyState.Move);
        }
    }
}