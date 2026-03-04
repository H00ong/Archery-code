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
        EnemyManager.Instance.RemoveEnemy(_ctx);
        
        _ctx.ColliderActive(false);
        _ctx.RigidbodyActive(false);
        
        if (_ctx.expItemPrefab != null)
        {
            SpawnExpItemAsync().Forget();
        }
    }
    
    private async Awaitable SpawnExpItemAsync()
    {
        GameObject expItem = null;

        try
        {
            if (!_poolManager.TryGetObject(_ctx.expItemPrefab, out expItem, _poolManager.extra))
            {
                expItem = await _poolManager.GetObjectAsync(_ctx.expItemPrefab, _poolManager.extra);
            }

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (expItem != null)
            {
                expItem.transform.position = transform.position;
                expItem.SetActive(true);
            }
        }
        catch (System.OperationCanceledException)
        {
            if (expItem != null)
                _poolManager.ReturnObject(expItem);

            return;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EnemyDie] 중 치명적 에러 발생: {ex.Message}");

            if (expItem != null)
                _poolManager.ReturnObject(expItem);

            return;
        }
    }
        
    public virtual void OnExit()
    {
    }

    public virtual void Tick()
    {

    }
}
