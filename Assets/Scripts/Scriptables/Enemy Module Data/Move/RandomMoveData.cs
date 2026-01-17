using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomMoveData", menuName = "Enemy/ModuleData/RandomMove")]
public class RandomMoveData : MoveModuleData
{
    [Header("Random Move Settings")]
    public float pickDirectionTime = 7f;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.RandomMove;
    }
    
    private void OnValidate()
    {
        targetTag = EnemyTag.RandomMove;
    }
}
