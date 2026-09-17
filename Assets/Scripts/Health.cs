using System;
using System.Collections.Generic;
using Effects;
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

    private int maxHealth = 100;
    private int currentHealth;
    private bool isLive = true;

    // 배럴 등으로 부여되는 보호막. 데미지를 체력보다 먼저 흡수한다.
    private int shield;
    // 이번에 채워진 보호막 최대치. Shield가 0이 되면 함께 리셋된다 (UI Slider 비율 기준).
    private int maxShield;

    /// <summary> 현재 체력 (UI 바인딩용 읽기 전용) </summary>
    public int CurrentHealth => currentHealth;

    /// <summary> 최대 체력 (UI 바인딩용 읽기 전용) </summary>
    public int MaxHealth => maxHealth;

    /// <summary> 현재 보호막 잔량 (UI 바인딩용 읽기 전용) </summary>
    public int Shield => shield;

    /// <summary> 이번에 채워진 보호막 최대치 (UI Slider maxValue 바인딩용 읽기 전용) </summary>
    public int MaxShield => maxShield;

    // 방어 스탯 캐싱
    private BaseStat _stat;
    // 특정 EffectType에 면역인 대상(보스 등)이면 해당 핵들러를 아예 적용하지 않는다.
    private ICrowdControlImmune _ccImmune;
    // Strategy 패턴: 이펙트 핸들러 리스트
    private readonly List<IEffectHandler> _effectHandlers = new()
    {
        new DotEffectHandler(EffectType.Fire),
        new DotEffectHandler(EffectType.Poison),
        new IceEffectHandler(),
        new LightningEffectHandler(),
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
        _ccImmune = GetComponent<ICrowdControlImmune>();
        InitializeEffectStates();

        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        shield = 0;
        maxShield = 0;

        isLive = true;
    }

    /// <summary> 보호막을 부여한다 (기존 보호막에 누적). 실드 배럴에서 사용. </summary>
    public void AddShield(int amount)
    {
        if (amount <= 0)
            return;
        shield += amount;
        maxShield = Mathf.Max(maxShield, shield);
    }

    /// <summary> 최대 체력을 늘리고 증가분만큼 현재 체력도 함께 회복한다. </summary>
    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0 || !isLive)
            return;

        maxHealth += amount;
        currentHealth += amount;
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
                handler.Tick(state, Time.deltaTime, _stat, OnStatusChanged, dmg => ApplyDotDamage(handler.Type, dmg));
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

        // 보호막이 있으면 체력보다 먼저 소모
        if (shield > 0)
        {
            int absorbed = Mathf.Min(shield, finalDamage);
            shield -= absorbed;
            finalDamage -= absorbed;
            if (shield <= 0)
                maxShield = 0;
        }

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
        ClearActiveEffectVisuals();
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

            if (_ccImmune != null && _ccImmune.IsImmuneTo(handler.Type))
                continue;

            if (effectStates.TryGetValue(handler.Type, out var state))
                handler.Apply(state, damageInfo, _stat, OnStatusChanged);
        }
    }

    private void ApplyDotDamage(EffectType type, float damage)
    {
        if (!isLive)
            return;

        int finalDamage = Mathf.RoundToInt(damage);

        if (shield > 0)
        {
            int absorbed = Mathf.Min(shield, finalDamage);
            shield -= absorbed;
            finalDamage -= absorbed;
            if (shield <= 0)
                maxShield = 0;
        }

        currentHealth -= finalDamage;

        // 플레이어의 Fire/Poison 도트 데미지만 확인용으로 로그 남김
        if ((type == EffectType.Fire || type == EffectType.Poison) && CompareTag(Utils.ToString(TagType.Player)))
            Debug.Log($"[Health] Player took {type} DOT damage: {finalDamage} (HP {currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isLive = false;
            ClearActiveEffectVisuals();
            OnDie?.Invoke();
            return;
        }
    }

    /// <summary>
    /// 사망 시 활성화된 상태이상 이패터를 강제로 끄고 비주얼을 원복한다.
    /// Update()가 isLive==false이면 더 이상 Tick을 돌리지 않아 EndStun/색상 복원이 영영 안 불리는 것을 방지한다.
    /// </summary>
    private void ClearActiveEffectVisuals()
    {
        foreach (var state in effectStates.Values)
        {
            if (!state.isActive)
                continue;
            state.isActive = false;
            OnStatusChanged?.Invoke(state.damageInfo, false);
        }
    }
}
