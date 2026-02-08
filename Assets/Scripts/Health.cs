using System;
using System.Collections;
using System.Collections.Generic;
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
    protected int currentHealth;
    protected bool isLive = true;

    // 효과 상태 Dictionary
    private Dictionary<EffectType, EffectState> effectStates;

    private void Awake()
    {
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

        ResetAllEffects();
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

        // 기본 데미지 적용
        int finalDamage = Mathf.RoundToInt(damageInfo.damageAmount);
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
    private void ResetAllEffects()
    {
        foreach (var state in effectStates.Values)
        {
            state.Reset();
        }
    }

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

        if (state.tickTimer >= state.damageInfo.tickInterval)
        {
            state.tickTimer -= state.damageInfo.tickInterval;
            ApplyDotDamage(state.damageInfo.dotDamageAmount);
        }

        if (state.timer >= state.damageInfo.effectDuration)
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
            state.isActive = true;
            state.damageInfo = damageInfo;
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

        if (state.timer >= state.damageInfo.effectDuration)
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
}
