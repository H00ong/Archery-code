using UnityEngine;

public interface IDamageable
{ 
    void TakeDamage(float damage, float modifier = 0f);
    bool IsDead();
}
