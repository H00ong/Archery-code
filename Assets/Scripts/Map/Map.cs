using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<Transform> spawnPointOfEnemies;
    public Transform playerPoint;
    public Transform portal;

    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask portalLayer;

    private void Start()
    {
        portal.gameObject.layer = obstacleLayer;
    }

    public void UpdatePortal()
    {
        portal.gameObject.layer = portalLayer;
    }
}
