using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "FollowMoveData", menuName = "Enemy/ModuleData/FollowMove")]
public class FollowMoveData : MoveModuleData
{
    [Header("Follow Move Settings")]
    public float defaultAttackRange = 2.0f;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.FollowMove;
    }
    
    private void OnValidate()
    {
        targetTag = EnemyTag.FollowMove;
    }
}
