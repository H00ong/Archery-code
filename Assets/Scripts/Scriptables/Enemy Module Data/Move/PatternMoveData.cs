using System.Collections.Generic;
using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternMoveData", menuName = "Enemy/ModuleData/PatternMove")]
public class PatternMoveData : MoveModuleData
{
    [Header("Pattern Move Settings")]
    public EnemyPointType patrolPointType;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.PatternMove;
    }
    
    private void OnValidate()
    {
        targetTag = EnemyTag.PatternMove;
        patrolPointType = EnemyPointType.PatrolPoint;
    }
    
    public List<Transform> GetPoints(EnemyController controller)
    {
        return controller.enemyReference != null 
            ? controller.enemyReference.GetPoints(EnemyPointType.PatrolPoint) 
            : null;
    }
}
