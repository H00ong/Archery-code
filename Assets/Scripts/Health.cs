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
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isLive = false;
            // Trigger death logic here, e.g., play animation, notify game manager, etc.
            Debug.Log($"{gameObject.name} has died.");
        }
        else
        {
            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");
        }
    }

    public virtual bool Heal(float healAmount, float modifier) 
    {
        if (!isLive)
            return false;

        if (currentHealth < maxHealth)
        {
            currentHealth += Mathf.RoundToInt(healAmount * (1 + modifier));
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            return true;
        }

        return false;
    }
}
