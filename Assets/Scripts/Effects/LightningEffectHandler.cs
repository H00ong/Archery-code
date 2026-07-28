using System;
using Effects;
using Stat;
using UnityEngine;

/// <summary>
/// Lightning 속성 이펙트 핸들러.
/// 맞으면 대상이 duration 동안 스턴(정지) 상태가 된다.
/// Player가 맞을 때는 지속시간이 적 대비 훨씬 짧게 적용된다.
/// </summary>
public class LightningEffectHandler : IEffectHandler
{
    /// <summary> 플레이어에게 적용될 때 duration에 곱해질 계수 (짧게 만들기 위함) </summary>
    private const float PlayerDurationScale = 0.4f;

    public EffectType Type => EffectType.Lightning;

    public void Apply(EffectState state, DamageInfo damageInfo, BaseStat stat, Action<DamageInfo, bool> onStatusChanged)
    {
        // Lightning EffectData가 없으면 아무 처리하지 않음 (플레이어 스탯에 정의 안 됨)
        var data = damageInfo.GetEffectData(EffectType.Lightning);
        if (data == null || data.duration <= 0f) return;

        if (!state.isActive)
        {
            state.isActive = true;
            state.damageInfo = damageInfo;
            onStatusChanged?.Invoke(state.damageInfo, true);

            var receiver = ResolveStunReceiver(stat);
            receiver?.BeginStun();
        }

        // 중첩 시 duration 재갱신
        state.timer = 0f;
    }

    public void Tick(EffectState state, float deltaTime, BaseStat stat, Action<DamageInfo, bool> onStatusChanged, Action<float> onDotDamage)
    {
        if (!state.isActive) return;

        state.timer += deltaTime;

        var data = state.damageInfo.GetEffectData(EffectType.Lightning);
        if (data == null)
        {
            EndStun(state, stat, onStatusChanged);
            return;
        }

        float duration = ResolveDuration(data.duration, stat);
        if (state.timer >= duration)
            EndStun(state, stat, onStatusChanged);
    }

    private void EndStun(EffectState state, BaseStat stat, Action<DamageInfo, bool> onStatusChanged)
    {
        state.isActive = false;

        var receiver = ResolveStunReceiver(stat);
        receiver?.EndStun();

        onStatusChanged?.Invoke(state.damageInfo, false);
    }

    /// <summary> 플레이어면 duration 축소, 아니면 그대로. </summary>
    private static float ResolveDuration(float baseDuration, BaseStat stat)
    {
        if (stat is PlayerStat)
            return baseDuration * PlayerDurationScale;
        return baseDuration;
    }

    /// <summary> 대상 게임오브젝트에서 IStunReceiver 컴포넌트를 찾는다. </summary>
    private static IStunReceiver ResolveStunReceiver(BaseStat stat)
    {
        if (stat == null) return null;
        return stat.GetComponent<IStunReceiver>();
    }
}
