using System;
using System.Collections.Generic;
using Stat;
using UnityEngine;

public class IceEffectHandler : IEffectHandler
{
    public EffectType Type => EffectType.Ice;

    public void Apply(EffectState state, DamageInfo damageInfo, BaseStat stat, Action<DamageInfo, bool> onStatusChanged)
    {
        if (!state.isActive)
        {
            state.isActive = true;
            state.damageInfo = GetCalculatedDamgeInfo(damageInfo, stat);
            onStatusChanged?.Invoke(state.damageInfo, true);
        }

        state.timer = 0f;
    }

    public void Tick(EffectState state, float deltaTime, BaseStat stat, Action<DamageInfo, bool> onStatusChanged, Action<float> onDotDamage)
    {
        if (!state.isActive)
            return;

        state.timer += deltaTime;

        var iceData = state.damageInfo.GetEffectData(EffectType.Ice);
        if (iceData == null) return;

        if (state.timer >= iceData.duration)
        {
            state.isActive = false;
            onStatusChanged?.Invoke(state.damageInfo, false);
        }
    }

    private DamageInfo GetCalculatedDamgeInfo(DamageInfo original, BaseStat stat)
    {
        var copy = new DamageInfo(original.damageAmount, original.type, original.attackSource);
        foreach (var kvp in original.effectDataMap)
            copy.effectDataMap[kvp.Key] = kvp.Value.Clone();

        var data = copy.GetEffectData(EffectType.Ice);
        if (data != null)
            data.value = ApplyMagicResistanceToEffect(data.value, stat);

        return copy;
    }

    private float ApplyMagicResistanceToEffect(float effectValue, BaseStat stat)
    {
        int mr = stat != null ? stat.MagicResistance : 0;
        if (mr <= 0) return effectValue;
        return effectValue * (80f / (80f + mr));
    }
}
