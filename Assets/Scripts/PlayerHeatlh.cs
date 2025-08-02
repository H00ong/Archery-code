using UnityEngine;

public class PlayerHeatlh : Health
{
    public override void TakeDamage(float damage, float modifier = 0)
    {
        base.TakeDamage(damage, modifier);
    }

    public override bool Heal(float healAmount, float modifier)
    {
        return base.Heal(healAmount, modifier);
    }
}
