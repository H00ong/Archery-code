using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Map
{
    public class GameMap : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        public Transform PlayerSpawnPoint => playerSpawnPoint;

        public Transform bossSpawnPoint;
        [SerializeField] private NavMeshSurface surface;

        public List<Transform> enemySpawnPoints;

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
