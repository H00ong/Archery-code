using Game.Enemies;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] EnemyController enemy;

    private void Start()
    {
        
    }

    void AttackTrigger()
    {
        enemy.SetAttackTrigger(true);
    }

    void HurtTrigger() 
    {
        enemy.SetHurtTrigger(true);
    }

    void AttackMoveTriggerActive() 
    {
        enemy.SetAttackMoveTrigger(true);
    }

    void AttackMoveTriggerDeactive()
    {
        enemy.SetAttackMoveTrigger(false);
    }

    public void Ability(string _tag) 
    {
        var tag = EnemyTagUtil.ParseTagsToMask(_tag);

        enemy.Ability(tag);
    }

    void Die() 
    {
        enemy.Die();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (enemy == null) enemy = GetComponentInParent<EnemyController>();
    }
#endif
}
