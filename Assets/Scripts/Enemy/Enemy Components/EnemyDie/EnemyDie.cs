using Enemy;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        var dropManager = DropManager.Instance;
        if (dropManager == null) return;

        bool isBoss = _ctx.IsBoss;

        var expPrefab = dropManager.ExpItemPrefab;
        if (expPrefab != null && expPrefab.RuntimeKeyIsValid() && dropManager.TryGetExpDropAmount(isBoss, out int expAmount))
        {
            SpawnDropAsync(expPrefab, expAmount, isExp: true).Forget();
        }

        var goldPrefab = dropManager.GoldItemPrefab;
        if (goldPrefab != null && goldPrefab.RuntimeKeyIsValid() && dropManager.TryGetGoldDropAmount(isBoss, out int goldAmount))
        {
            SpawnDropAsync(goldPrefab, goldAmount, isExp: false).Forget();
        }
    }

    private async Awaitable SpawnDropAsync(AssetReferenceGameObject prefab, int amount, bool isExp)
    {
        GameObject item = null;

        try
        {
            if (!_poolManager.TryGetObject(prefab, out item, _poolManager.extra))
            {
                item = await _poolManager.GetObjectAsync(prefab, _poolManager.extra);
            }

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (item == null) return;

            if (isExp)
            {
                if (item.TryGetComponent<ExpItem>(out var exp))
                    exp.SetAmount(amount);
            }
            else
            {
                if (item.TryGetComponent<GoldItem>(out var gold))
                    gold.SetAmount(amount);
            }

            float yOffset = MapManager.Instance != null ? MapManager.Instance.GetItemDropYOffset() : 0f;
            item.transform.position = transform.position + Vector3.up * yOffset;
            item.SetActive(true);
        }
        catch (System.OperationCanceledException)
        {
            if (item != null)
                _poolManager.ReturnObject(item);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EnemyDie] 드롭 스폰 중 에러: {ex.Message}");
            if (item != null)
                _poolManager.ReturnObject(item);
        }
    }

    public virtual void OnExit() { }

    public virtual void Tick() { }
}
