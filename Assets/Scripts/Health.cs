using System.Xml.Schema;
using UnityEngine;

public abstract class Health : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    protected int currentHealth;
    protected bool isLive = true;

    public void InitializeHealth(int maxHealth = 100)
    {
        currentHealth = maxHealth;
        isLive = true;
    }

    public bool IsDead()
    {
        return !isLive;
    }

    public virtual void TakeDamage(float damage, float modifier = 0f)
    {
        if (!isLive) return;

        currentHealth -= Mathf.RoundToInt(damage * (1 + modifier));
        
        if (currentHealth > 0) return;
        
        currentHealth = 0;
        isLive = false;
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
