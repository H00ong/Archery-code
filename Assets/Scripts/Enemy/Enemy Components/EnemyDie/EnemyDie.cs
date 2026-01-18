using Enemy;
using Managers;
using UnityEngine;

public class EnemyDie : MonoBehaviour, IEnemyBehavior
{
    private EnemyController _ctx;
    private PoolManager _poolManager;
    
    public virtual void Init(EnemyController ctx, BaseModuleData data = null)
    {
        _ctx = ctx;
        _poolManager = PoolManager.Instance;
    }

    public virtual void OnEnter() 
    {
        _ctx.ColliderActive(false);
        _ctx.RigidbodyActive(false);
        
        if (_ctx.expItemPrefab != null)
        {
            StartCoroutine(SpawnExpItem());
        }
    }
    
    private System.Collections.IEnumerator SpawnExpItem()
    {
        if (!_poolManager.TryGetObject(_ctx.expItemPrefab, out GameObject expItem, _poolManager.Extra))
        {
            yield return _poolManager.GetObject(_ctx.expItemPrefab, inst => expItem = inst, _poolManager.Extra);
        }
        
        if (expItem != null)
        {
            expItem.transform.position = transform.position;
            expItem.SetActive(true);
        }
    }
        
    public virtual void OnExit()
    {
        
    }

    public virtual void Tick()
    {

    }
}
