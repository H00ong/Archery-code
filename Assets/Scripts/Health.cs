using System;
using System.Xml.Schema;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public event Action OnDie;
    public event Action OnHit;

    public int maxHealth = 100;
    protected int currentHealth;
    protected bool isLive = true;

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

    public void TakeDamage(float damage, float modifier = 0f)
    {
        if (!isLive) return;

        currentHealth -= Mathf.RoundToInt(damage * (1 + modifier));

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

            if(currentHealth > maxHealth)
                currentHealth = maxHealth;

            valid = true;
        }

        valid = false;
    }
}
