using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<Transform> spawnPointOfEnemies;
    public Transform playerPoint;
    [SerializeField] GameObject portal;
    [SerializeField] Collider portalCollider;

    readonly string obstacleLayerName = "Obstacle";
    readonly string portalLayerName   = "Portal";

    public void Init() 
    {
        StageManager.OnStageCleared += ActivePortal;

        foreach (var point in spawnPointOfEnemies) 
            point.gameObject.SetActive(false);

        playerPoint.gameObject.SetActive(false);
        portal.GetOrAddComponent<PlayerPortal>();
        InActivePortal();
    }

    public void ActivePortal()
    {
        portal.layer = LayerMask.NameToLayer(portalLayerName);
        portalCollider.isTrigger = true;
    }

    public void InActivePortal() 
    {
        portal.layer = LayerMask.NameToLayer(obstacleLayerName);
        portalCollider.isTrigger = false;
    }

    private void OnDisable()
    {
        StageManager.OnStageCleared -= ActivePortal;        
    }
}
