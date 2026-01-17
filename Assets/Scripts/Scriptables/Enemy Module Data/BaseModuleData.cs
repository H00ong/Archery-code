using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseModule", menuName = "Enemy/ModuleData/BaseModule")]
public class BaseModuleData : ScriptableObject
{
    [Header("Identification")]
    public EnemyName targetName;    
    public EnemyTag targetTag;
}
