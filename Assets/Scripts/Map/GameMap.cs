using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace Map
{
    public class GameMap : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public Transform bossSpawnPoint;
        [SerializeField] private NavMeshSurface surface;

        public List<Transform> enemySpawnPoints;

        public void Init() 
        {
            foreach (var point in enemySpawnPoints) 
                point.gameObject.SetActive(false);

            PlayerSpawnPoint.gameObject.SetActive(false);
        }

        public Bounds GetBounds() => surface.navMeshData.sourceBounds;
    }
}
