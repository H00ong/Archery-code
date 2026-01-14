using System;
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "FlyingShootData", menuName = "Enemy/ModuleData/FlyingShootData")]
public class FlyingShootData : BaseModuleData
{
    [Header("Assets (Addressable)")]
    public AssetReferenceGameObject projectilePrefab; // 투사체 프리팹 참조

    [Header("Firing Settings")]
    public EnemyPointType firePointType;
    
    // [자동 설정] 이 SO를 생성하면 자동으로 태그가 FlyingShoot로 설정됨 (실수 방지)
    private void OnEnable()
    {
        linkedTag = EnemyTag.FlyingShoot;
    }
    
    public List<Transform> GetShootingPoint(EnemyController controller)
    {
        // 1. 컨트롤러에 ReferenceHub가 있고, 초기화가 되어 있다면 요청
        return controller.enemyReference != null ? controller.enemyReference.GetPoints(EnemyPointType.FlyingShootingMuzzle) : null;
    }

    private void OnValidate()
    {
        linkedTag = EnemyTag.FlyingShoot;
        firePointType = EnemyPointType.FlyingShootingMuzzle;
    }
}
