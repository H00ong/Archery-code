using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public event Action OnDie;
    public event Action OnHit;

    public int maxHealth = 100;
    protected int currentHealth;
    protected bool isLive = true;

    // 상태이상 관련
    private Coroutine _dotCoroutine;
    private float _currentSlowRate = 0f;

    public float CurrentSlowRate => _currentSlowRate;

    public void InitializeHealth(int maxHealth = 100)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        isLive = true;
        _currentSlowRate = 0f;
    }

    public bool IsDead()
    {
        return !isLive;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!isLive) return;

        // 기본 데미지 적용
        int finalDamage = Mathf.RoundToInt(damageInfo.amount);
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

    private void ApplyDamageEffect(DamageInfo damageInfo)
    {
        switch (damageInfo.type)
        {
            case EffectType.Fire:
                if (_dotCoroutine != null) StopCoroutine(_dotCoroutine);
                _dotCoroutine = StartCoroutine(FireDotCoroutine(damageInfo.effectDuration, damageInfo.effectValue));
                break;

            case EffectType.Venom:
                if (_dotCoroutine != null) StopCoroutine(_dotCoroutine);
                _dotCoroutine = StartCoroutine(VenomDotCoroutine(damageInfo.effectDuration, damageInfo.effectValue));
                break;

            case EffectType.Ice:
                StartCoroutine(IceSlowCoroutine(damageInfo.effectDuration, damageInfo.effectValue));
                break;

            case EffectType.Normal:
            default:
                break;
        }
    }

    private IEnumerator FireDotCoroutine(float duration, float damagePerTick)
    {
        float elapsed = 0f;
        float tickInterval = 0.3f; // 빠른 틱

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (!isLive) yield break;

            currentHealth -= Mathf.RoundToInt(damagePerTick);
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isLive = false;
                OnDie?.Invoke();
                yield break;
            }
            OnHit?.Invoke();
        }
        _dotCoroutine = null;
    }

    private IEnumerator VenomDotCoroutine(float duration, float damagePerTick)
    {
        float elapsed = 0f;
        float tickInterval = 1.0f; // 느린 틱

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (!isLive) yield break;

            currentHealth -= Mathf.RoundToInt(damagePerTick);
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isLive = false;
                OnDie?.Invoke();
                yield break;
            }
            OnHit?.Invoke();
        }
        _dotCoroutine = null;
    }

    private IEnumerator IceSlowCoroutine(float duration, float slowRate)
    {
        _currentSlowRate = slowRate;
        yield return new WaitForSeconds(duration);
        _currentSlowRate = 0f;
    }

    public virtual void TakeHeal(int amount, out bool valid) 
    {
        if (!isLive)
            valid = false;

        if (currentHealth < maxHealth)
        {
            currentHealth += amount;

            if(currentHealth > maxHealth)
                currentHealth = maxHealth;

            valid = true;
        }

        valid = false;
    }
}
