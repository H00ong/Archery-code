using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.AI;

namespace Map
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyIdentity identity;
        public Transform spawnPoint;
    }

    public class GameMap : MonoBehaviour
    {
        private const float offsetY = 0.5f;

        [Header("Map Info")]
        [SerializeField] private Transform floor;

        [Header("Surface")]
        [SerializeField] private NavMeshSurface surface;

        [Header("Player Spawn Point")]
        [SerializeField] private Transform playerSpawnPoint;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        [Header("Enemy Spawn Points")]
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private List<Transform> enemySpawnPoints;
        public Transform BossSpawnPoint => bossSpawnPoint;
        public List<Transform> EnemySpawnPoints => enemySpawnPoints;

        [Header("Predefined Enemies (고정 배치)")]
        [SerializeField] private List<EnemySpawnData> predefinedEnemies;
        public List<EnemySpawnData> PredefinedEnemies => predefinedEnemies;

        [Header("Patrol")]
        [SerializeField] private List<PatrolPoint> patrolPoints;

        public void Init()
        {
            foreach (var point in enemySpawnPoints)
                point.gameObject.SetActive(false);

            PlayerSpawnPoint.gameObject.SetActive(false);
        }

        public List<Vector3> GetPatrolPositions()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
                return new List<Vector3>();

            int idx = Random.Range(0, patrolPoints.Count);
            return patrolPoints[idx].GetPatrolPositions();
        }

        public List<PatrolPoint> GetAllPatrolPoints() => patrolPoints;

        public Vector3 GetRandomNavMeshPoint()
        {
            var bounds = surface.navMeshData.sourceBounds;
            var floorPos = new Vector3(floor.position.x,
                                        floor.position.y + offsetY, 
                                        floor.position.z);

            for (int i = 0; i < 30; i++)
            {
                var randomPoint = new Vector3(
                    Random.Range(-bounds.extents.x, bounds.extents.x),
                    floorPos.y,
                    Random.Range(-bounds.extents.z, bounds.extents.z)
                );

                if (NavMesh.SamplePosition(randomPoint, out var hit, 5f, NavMesh.AllAreas))
                    return new Vector3(hit.position.x, floorPos.y, hit.position.z);
            }

            return floorPos;
        }
    }
}
