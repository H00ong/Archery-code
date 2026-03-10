using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Map
{
    public class GameMap : MonoBehaviour
    {
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

        [Header("Patrol")]
        [SerializeField] private List<PatrolPoint> patrolPoints;

        private NavMeshTriangulation _cachedTriangulation;

        private void OnEnable()
        {
            _cachedTriangulation = NavMesh.CalculateTriangulation();
        }

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
            int triangleCount = _cachedTriangulation.indices.Length / 3;
            int triIndex = Random.Range(0, triangleCount) * 3;

            Vector3 a = _cachedTriangulation.vertices[_cachedTriangulation.indices[triIndex]];
            Vector3 b = _cachedTriangulation.vertices[_cachedTriangulation.indices[triIndex + 1]];
            Vector3 c = _cachedTriangulation.vertices[_cachedTriangulation.indices[triIndex + 2]];

            // barycentric random point inside triangle
            float r1 = Mathf.Sqrt(Random.value);
            float r2 = Random.value;
            return (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c;
        }
    }
}
