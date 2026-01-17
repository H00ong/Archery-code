using Enemy;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    private EnemyController _enemy => GetComponentInParent<EnemyController>();

    private void AttackEndTrigger()
    {
        _enemy.SetAttackEndTrigger(true);
    }

    private void HurtTrigger() 
    {
        _enemy.SetHurtEndTrigger(true);
    }

    private void AttackMoveTriggerActive() 
    {
        _enemy.SetAttackMoveTrigger(true);
    }

    public void AttackMoveTriggerDeactive()
    {
        _enemy.SetAttackMoveTrigger(false);
    }

    public void Ability(string tagOfString) 
    {
        var tag = EnemyTagUtil.ParseTagsToMask(tagOfString);

        _enemy.Ability(tag);
    }

    private void Die() 
    {
        _enemy.Die();
    }
}
