using System;
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "FlyingShootData", menuName = "Enemy/ModuleData/FlyingShootData")]
public class FlyingShootData : AttackModuleData
{
    [Header("Assets (Addressable)")]
    public AssetReferenceGameObject projectilePrefab;

    [Header("Firing Settings")]
    public EnemyPointType firePointType;
    
    private void OnEnable()
    {
        targetTag = EnemyTag.FlyingShoot;
    }
    
    public List<Transform> GetShootingPoint(EnemyController controller)
    {
        return controller.enemyReference != null ? controller.enemyReference.GetPoints(EnemyPointType.FlyingShootingMuzzle) : null;
    }

    private void OnValidate()
    {
        targetTag = EnemyTag.FlyingShoot;
        firePointType = EnemyPointType.FlyingShootingMuzzle;
    }
}
