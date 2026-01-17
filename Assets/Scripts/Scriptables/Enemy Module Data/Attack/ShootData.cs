using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ShootData", menuName = "Enemy/ModuleData/ShootData")]
public class ShootData : AttackModuleData
{
    [Header("Assets (Addressable)")]
    public AssetReferenceGameObject projectilePrefab;

    [Header("Firing Settings")]
    public EnemyPointType firePointType;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.Shoot;
    }
    
    public List<Transform> GetShootingPoint(EnemyController controller)
    {
        return controller.enemyReference != null ? controller.enemyReference.GetPoints(EnemyPointType.NormalShootingMuzzle) : null;
    }

    private void OnValidate()
    {
        targetTag = EnemyTag.Shoot;
        firePointType = EnemyPointType.NormalShootingMuzzle;
    }
}
