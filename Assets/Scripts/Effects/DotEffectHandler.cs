using System;
using Stat;
using UnityEngine;

public class DotEffectHandler : IEffectHandler
{
    public EffectType Type { get; }

    public DotEffectHandler(EffectType type)
    {
        Type = type;
    }

    public void Apply(EffectState state, DamageInfo damageInfo, BaseStat stat, Action<DamageInfo, bool> onStatusChanged)
    {
        if (!state.isActive)
        {
            state.isActive = true;
            state.damageInfo = damageInfo;
            onStatusChanged?.Invoke(state.damageInfo, true);
        }

        state.timer = 0f;
        state.tickTimer = 0f;
        state.damageInfo = damageInfo;
    }

    public void Tick(EffectState state, float deltaTime, BaseStat stat, Action<DamageInfo, bool> onStatusChanged, Action<float> onDotDamage)
    {
        if (!state.isActive)
            return;

        state.timer += deltaTime;
        state.tickTimer += deltaTime;

        var effectData = state.damageInfo.GetEffectData(Type);
        if (effectData == null) return;

        if (state.tickTimer >= effectData.tickInterval)
        {
            state.tickTimer -= effectData.tickInterval;

            float dotDmg = ApplyMagicResistanceToDot(effectData.dotDamage, stat);
            onDotDamage?.Invoke(dotDmg);
        }

        if (state.timer >= effectData.duration)
        {
            state.isActive = false;
            onStatusChanged?.Invoke(state.damageInfo, false);
        }
    }

    private float ApplyMagicResistanceToDot(float dotDamage, BaseStat stat)
    {
        int mr = stat != null ? stat.MagicResistance : 0;
        if (mr <= 0) return dotDamage;
        return dotDamage * (50f / (50f + mr));
    }
}
