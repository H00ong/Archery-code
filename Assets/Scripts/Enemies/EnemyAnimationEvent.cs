using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    Enemy enemy;
    [SerializeField] Transform[] shootingPos;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void AnimationTrigger() 
    {
        enemy.AttackTrigger();
    }

    void HurtTrigger() 
    {
        enemy.HurtTrigger();
    }

    void AttackMoveTriggerActive() 
    { 
        MeleeEnemy meleeEnemy = GetComponentInParent<MeleeEnemy>();

        meleeEnemy?.AttackMoveTrigger(true);
    }

    void AttackMoveTriggerDeactive()
    {
        MeleeEnemy meleeEnemy = GetComponentInParent<MeleeEnemy>();

        meleeEnemy?.AttackMoveTrigger(false);
    }

    void Shoot() 
    {
        if(shootingPos == null)
            return;

        RangedEnemy rangedEnemy = GetComponentInParent<RangedEnemy>();

        foreach (Transform pos in shootingPos)
        {
            rangedEnemy?.Shoot(pos);
        }
    }

    void Die() 
    {
        enemy.Die();
    }
}
