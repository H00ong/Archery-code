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

    // Strategy 패턴: 이펙트 핸들러 리스트
    private readonly List<IEffectHandler> _effectHandlers = new()
    {
        new DotEffectHandler(EffectType.Fire),
        new DotEffectHandler(EffectType.Poison),
        new IceEffectHandler(),
    };

    // 효과 상태 Dictionary
    private Dictionary<EffectType, EffectState> effectStates;

    private void InitializeEffectStates()
    {
        effectStates = new Dictionary<EffectType, EffectState>();
        foreach (var handler in _effectHandlers)
            effectStates[handler.Type] = new EffectState();
    }
    
    public void InitializeHealth(int maxHealth = 100)
    {
        _stat = GetComponent<BaseStat>();
        InitializeEffectStates();

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

        foreach (var handler in _effectHandlers)
        {
            if (effectStates.TryGetValue(handler.Type, out var state))
                handler.Tick(state, Time.deltaTime, _stat, OnStatusChanged, ApplyDotDamage);
        }
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
        ApplyEffect(damageInfo);

        if (currentHealth > 0)
        {
            OnHit?.Invoke();
            return;
        }

        currentHealth = 0;
        isLive = false;
        OnDie?.Invoke();
    }

    public virtual bool TryTakeHeal(int amount)
    {
        if (!isLive)
            return false;

        if (currentHealth >= maxHealth)
            return false;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        return true;
    }

    private void ApplyEffect(DamageInfo damageInfo)
    {
        foreach (var handler in _effectHandlers)
        {
            if (!Utils.HasEffectType(damageInfo.type, handler.Type))
                continue;

            if (effectStates.TryGetValue(handler.Type, out var state))
                handler.Apply(state, damageInfo, _stat, OnStatusChanged);
        }
    }

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
}
