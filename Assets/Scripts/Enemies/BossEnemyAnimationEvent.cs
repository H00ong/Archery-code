using UnityEngine;

public class BossEnemyAnimationEvent : MonoBehaviour
{
    BossEnemy bossEnemy;

    [SerializeField] Transform[] flyingShootingPoints;
    [SerializeField] Transform[] shootingPoints;

    void Start()
    {
        bossEnemy = GetComponentInParent<BossEnemy>();
    }

    void AnimationTrigger()
    {
        bossEnemy.AttackTrigger();
    }

    void AttackMoveTriggerActive()
    {
        // bossEnemy.AttackMoveTrigger(true);
    }

    void AttackMoveTriggerDeactive()
    {
        // bossEnemy.AttackMoveTrigger(true);
    }

    void FlyingShoot() 
    {
        foreach (Transform pos in flyingShootingPoints)
        {
            bossEnemy.FlyingTargetingShoot(pos);
        }
    }

    void Shoot() 
    {
        foreach (Transform pos in shootingPoints)
        {
            bossEnemy.Shoot(pos);
        }
    }

    void Die()
    {
        bossEnemy.Die();
    }
}
