using System;
using System.Collections.Generic;
using Stat;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    bool IsDead();
}

public class EffectState
{
    public bool isActive;
    public float timer;
    public float tickTimer;
    public DamageInfo damageInfo;

    public EffectState()
    {
        Reset();
    }

    public void Reset()
    {
        isActive = false;
        timer = 0f;
        tickTimer = 0f;
        damageInfo = default;
    }
}

public class Health : MonoBehaviour, IDamageable
{
    public event Action OnDie;
    public event Action OnHit;
    public event Action<DamageInfo, bool> OnStatusChanged;

    public int maxHealth = 100;
    private int currentHealth;
    private bool isLive = true;

    // 방어 스탯 캐싱
    private BaseStat _stat;

    // 효과 상태 Dictionary
    private Dictionary<EffectType, EffectState> effectStates;

    private void Awake()
    {
        _stat = GetComponent<BaseStat>();
        InitializeEffectStates();
    }

    private void InitializeEffectStates()
    {
        effectStates = new Dictionary<EffectType, EffectState>
        {
            { EffectType.Fire, new() },
            { EffectType.Poison, new() },
            { EffectType.Ice, new() },
        };
    }

    public void InitializeHealth(int maxHealth = 100)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;

        isLive = true;
    }

    public bool IsDead()
    {
        return !isLive;
    }

    private void Update()
    {
        if (!isLive)
            return;

        // 도트 효과 업데이트
        foreach (var effectType in Utils.DotEffectTypes)
        {
            UpdateDotEffect(effectType);
        }

        UpdateIceEffect();
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!isLive)
            return;

        // 물리 방어력 적용: finalDamage = damage * (100 / (100 + armor))
        int armor = _stat != null ? _stat.Armor : 0;
        float reduced = damageInfo.damageAmount * (100f / (100f + armor));
        int finalDamage = Mathf.RoundToInt(reduced);
        currentHealth -= finalDamage;

        // 속성별 효과 적용
        ApplyDamageEffect(damageInfo);

        if (currentHealth > 0)
        {
            OnHit?.Invoke();
            return;
        }

        currentHealth = 0;
        isLive = false;
        OnDie?.Invoke();
    }

    public virtual void TakeHeal(int amount, out bool valid)
    {
        if (!isLive)
            valid = false;

        if (currentHealth < maxHealth)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            valid = true;
        }

        valid = false;
    }

    private void ApplyDamageEffect(DamageInfo damageInfo)
    {
        foreach (var effectType in Utils.DotEffectTypes)
        {
            if (Utils.HasEffectType(damageInfo.type, effectType))
            {
                ApplyDotEffect(damageInfo);
            }
        }

        if (Utils.HasEffectType(damageInfo.type, EffectType.Ice))
        {
            ApplyIceEffect(damageInfo);
        }
    }

    #region Effect

    private void ApplyDotEffect(DamageInfo damageInfo)
    {
        if (!effectStates.TryGetValue(damageInfo.type, out var state))
            return;

        if (!state.isActive)
        {
            state.isActive = true;
            state.damageInfo = damageInfo;
            OnStatusChanged?.Invoke(state.damageInfo, true);
        }

        state.timer = 0f;
        state.tickTimer = 0f;
        state.damageInfo = damageInfo;
    }

    private void UpdateDotEffect(EffectType type)
    {
        if (!effectStates.TryGetValue(type, out var state))
            return;
        if (!state.isActive)
            return;

        state.timer += Time.deltaTime;
        state.tickTimer += Time.deltaTime;

        var effectData = state.damageInfo.GetEffectData(type);
        if (effectData == null) return;

        if (state.tickTimer >= effectData.tickInterval)
        {
            state.tickTimer -= effectData.tickInterval;

            // 마법 저항력으로 DOT 데미지 감소 (약한 공식: 50 / (50 + MR))
            float dotDmg = ApplyMagicResistanceToDot(effectData.dotDamage);
            ApplyDotDamage(dotDmg);
        }

        if (state.timer >= effectData.duration)
        {
            state.isActive = false;
            OnStatusChanged?.Invoke(state.damageInfo, false);
        }
    }

    // Ice 효과 적용
    private void ApplyIceEffect(DamageInfo damageInfo)
    {
        if (!effectStates.TryGetValue(EffectType.Ice, out var state))
            return;

        if (!state.isActive)
        {
            // MR로 슬로우 효과 감소 적용한 DamageInfo 저장
            state.isActive = true;
            state.damageInfo = CreateMRReducedDamageInfo(damageInfo, EffectType.Ice);
            OnStatusChanged?.Invoke(state.damageInfo, true);
        }

        state.timer = 0f;
    }

    // Ice 효과 업데이트
    private void UpdateIceEffect()
    {
        if (!effectStates.TryGetValue(EffectType.Ice, out var state))
            return;
        if (!state.isActive)
            return;

        state.timer += Time.deltaTime;

        var iceData = state.damageInfo.GetEffectData(EffectType.Ice);
        
        if (iceData == null) return;

        if (state.timer >= iceData.duration)
        {
            state.isActive = false;
            OnStatusChanged?.Invoke(state.damageInfo, false);
        }
    }

    // 도트 데미지 적용 공통 함수
    private void ApplyDotDamage(float damage)
    {
        if (!isLive)
            return;

        currentHealth -= Mathf.RoundToInt(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isLive = false;
            OnDie?.Invoke();
            return;
        }
    }
    #endregion

    #region Damage Reduction

    /// <summary>
    /// DamageInfo의 특정 EffectType에 대해 MR 감소를 적용한 복사본을 만든다.
    /// Ice의 경우 effectValue(슬로우 비율)를 줄인다.
    /// </summary>
    private DamageInfo CreateMRReducedDamageInfo(DamageInfo original, EffectType effectType)
    {
        var copy = new DamageInfo(original.damageAmount, original.type, original.attackSource);
        // 원본의 effectDataMap을 복사
        foreach (var kvp in original.effectDataMap)
            copy.effectDataMap[kvp.Key] = kvp.Value.Clone();

        var data = copy.GetEffectData(effectType);
        if (data != null)
            data.value = ApplyMagicResistanceToEffect(data.value);

        return copy;
    }

    /// <summary>
    /// DOT 데미지에 마법 저항력 적용 (약한 공식).
    /// reducedDot = dot * (50 / (50 + MR))
    /// 일반 방어 공식(100 기준)보다 감소 폭이 작아 DOT가 지나치게 약해지지 않는다.
    /// </summary>
    private float ApplyMagicResistanceToDot(float dotDamage)
    {
        int mr = _stat != null ? _stat.MagicResistance : 0;
        if (mr <= 0) return dotDamage;
        return dotDamage * (50f / (50f + mr));
    }

    /// <summary>
    /// Ice 슬로우 등 효과 수치에 마법 저항력 적용.
    /// reducedValue = value * (80 / (80 + MR))
    /// MR이 높아도 슬로우가 완전히 사라지지 않되 체감 가능하게 줄어든다.
    /// 예) MR=20 → 0.3 슬로우 → 약 0.24 (≈20% 감소)
    /// </summary>
    private float ApplyMagicResistanceToEffect(float effectValue)
    {
        int mr = _stat != null ? _stat.MagicResistance : 0;
        if (mr <= 0) return effectValue;
        return effectValue * (80f / (80f + mr));
    }

    #endregion
}
