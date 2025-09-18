using NUnit.Framework;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.Build;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<Transform> SpawnPointOfEnemies;

    [SerializeField] Transform _playerPoint;
    public Transform playerPoint => _playerPoint;

    [SerializeField] GameObject _portal;
    [SerializeField] Collider _portalCollider;
    [SerializeField] NavMeshSurface _surface;

    readonly string obstacleLayerName = "Obstacle";
    readonly string portalLayerName   = "Portal";

    public void Init() 
    {
        StageManager.OnStageCleared += ActivePortal;

        foreach (var point in SpawnPointOfEnemies) 
            point.gameObject.SetActive(false);

        playerPoint.gameObject.SetActive(false);
        _portal.GetOrAddComponent<PlayerPortal>();
        InActivePortal();
    }

    public void ActivePortal()
    {
        _portal.layer = LayerMask.NameToLayer(portalLayerName);
        _portalCollider.isTrigger = true;
    }

    public void InActivePortal() 
    {
        _portal.layer = LayerMask.NameToLayer(obstacleLayerName);
        _portalCollider.isTrigger = false;
    }

    private void OnDisable()
    {
        StageManager.OnStageCleared -= ActivePortal;        
    }

    public Bounds GetBounds() => _surface.navMeshData.sourceBounds;
}
