using Enemies;
using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseModule", menuName = "Enemy/ModuleData/BaseModule")]
public class BaseModuleData : ScriptableObject
{
    public EnemyName targetName;    
    public EnemyTag linkedTag;
}
