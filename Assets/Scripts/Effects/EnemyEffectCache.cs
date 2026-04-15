using System.Collections.Generic;
using Map;
using Stat;
using UnityEngine;

/// <summary>
/// 맵의 enemyEffects(mapData.json)를 static으로 캐시.
/// 맵 시작 시 Init, 맵 종료 시 Clear.
/// EnemyStat, DamageInfo 등에서 참조.
/// </summary>
public static class EnemyEffectCache
{
    private static Dictionary<EffectType, EffectData> _cache;

    public static bool IsLoaded => _cache != null;

    /// <summary>
    /// MapData의 enemyEffects로부터 EffectType별 EffectData를 캐시한다.
    /// 게임 맵 진입 시 한 번 호출.
    /// </summary>
    public static void Init(List<EffectConfig> effectConfigs)
    {
        _cache = new Dictionary<EffectType, EffectData>();

        if (effectConfigs == null) return;

        foreach (var cfg in effectConfigs)
        {
            _cache[cfg.effectType] = new EffectData(
                cfg.duration,
                cfg.effectValue,
                cfg.damagePerTick,
                cfg.tickInterval
            );
        }

        Debug.Log($"[EnemyEffectCache] Loaded {_cache.Count} effect(s).");
    }

    /// <summary>
    /// 맵 종료 시 캐시 해제.
    /// </summary>
    public static void Clear()
    {
        _cache = null;
    }

    /// <summary>
    /// 특정 EffectType의 EffectData를 반환. 없으면 null.
    /// </summary>
    public static EffectData Get(EffectType type)
    {
        if (_cache != null && _cache.TryGetValue(type, out var data))
            return data;
        return null;
    }

    /// <summary>
    /// 전체 캐시된 EffectData 맵 반환 (복사본).
    /// </summary>
    public static Dictionary<EffectType, EffectData> GetAll()
    {
        if (_cache == null) return new Dictionary<EffectType, EffectData>();

        var copy = new Dictionary<EffectType, EffectData>();
        foreach (var kvp in _cache)
            copy[kvp.Key] = kvp.Value.Clone();
        return copy;
    }
}
