using UnityEngine;

public class EnemyHealth : Health
{
    public override void TakeDamage(float damage, float modifier = 0)
    {
        base.TakeDamage(damage, modifier);
        // Additional enemy-specific logic can be added here, e.g., play hurt animation
    }

    public override bool Heal(float healAmount, float modifier)
    {
        return base.Heal(healAmount, modifier);
        // Additional enemy-specific logic can be added here, e.g., play heal animation
    }
}
