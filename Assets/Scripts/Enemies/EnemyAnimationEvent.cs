using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void AnimationTrigger() 
    {
        enemy.AnimationTrigger();
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
}
