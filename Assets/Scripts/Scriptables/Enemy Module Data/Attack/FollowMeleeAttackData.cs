using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "FollowMeleeAttackData", menuName = "Enemy/ModuleData/FollowMeleeAttack")]
public class FollowMeleeAttackData : MeleeAttackData
{
    [Header("Follow Melee Attack Settings")]
    public float attackRange = 1.5f;
    public float chaseDuration = 3f;
    public float moveSpeedIncreaseMultiplier = 1.2f;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.FollowMeleeAttack;
    }
    
    private void OnValidate()
    {
        targetTag = EnemyTag.FollowMeleeAttack;
    }
}
