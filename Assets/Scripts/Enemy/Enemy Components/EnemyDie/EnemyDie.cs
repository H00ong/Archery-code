using Enemy;
using Managers;
using Map;
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

        var mapData = MapManager.Instance != null ? MapManager.Instance.CurrentMapData : null;
        bool isBoss = _ctx.IsBoss;

        if (_ctx.expItemPrefab != null && _ctx.expItemPrefab.RuntimeKeyIsValid())
        {
            int expAmount = ComputeAmount(_ctx.baseExpAmount, mapData?.expDrop, isBoss);
            SpawnDropAsync(_ctx.expItemPrefab, expAmount, isExp: true).Forget();
        }

        if (_ctx.goldItemPrefab != null && _ctx.goldItemPrefab.RuntimeKeyIsValid())
        {
            int goldAmount = ComputeAmount(_ctx.baseGoldAmount, mapData?.goldDrop, isBoss);
            SpawnDropAsync(_ctx.goldItemPrefab, goldAmount, isExp: false).Forget();
        }
    }

    private static int ComputeAmount(int baseAmount, DropConfig cfg, bool isBoss)
    {
        if (baseAmount <= 0) return 0;
        if (cfg == null) return baseAmount;

        float roll = Random.Range(cfg.minRandom, cfg.maxRandom);
        float mul = cfg.multiplier * roll;
        if (isBoss && cfg.bossMultiplier > 0f) mul *= cfg.bossMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(baseAmount * mul));
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

            item.transform.position = transform.position;
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
