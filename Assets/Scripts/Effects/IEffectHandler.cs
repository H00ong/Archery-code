using System;
using Stat;

public interface IEffectHandler
{
    EffectType Type { get; }
    void Apply(EffectState state, DamageInfo damageInfo, BaseStat stat, Action<DamageInfo, bool> onStatusChanged);
    void Tick(EffectState state, float deltaTime, BaseStat stat, Action<DamageInfo, bool> onStatusChanged, Action<float> onDotDamage);
}
