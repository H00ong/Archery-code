using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackData", menuName = "Enemy/ModuleData/MeleeAttack")]
public class MeleeAttackData : AttackModuleData
{
    [Header("Melee Attack Settings")]
    public float attackMoveSpeed = 3f;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.MeleeAttack;
    }
    
    private void OnValidate()
    {
        targetTag = EnemyTag.MeleeAttack;
    }
}

