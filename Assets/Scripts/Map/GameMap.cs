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

        [Header("Map Creation Guide")]
        [Tooltip("선택 사항입니다. 지정하지 않으면 Floor의 Bounds 중심을 자동으로 사용합니다.")]
        [SerializeField] private Transform mapGuideCenter;
        [Tooltip("새 맵 제작 시 공통으로 사용할 가로(X), 높이(Y), 세로(Z) 크기입니다.")]
        [SerializeField] private Vector3 mapGuideSize = new Vector3(22f, 6.48f, 36f);

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

        [Header("Item Drop")]
        [SerializeField] private float itemDropYOffset = 0.5f;
        public float ItemDropYOffset => itemDropYOffset;

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

        private void OnDrawGizmos()
        {
            Vector3 guideSize = new Vector3(
                Mathf.Max(0.1f, mapGuideSize.x),
                Mathf.Max(0.1f, mapGuideSize.y),
                Mathf.Max(0.1f, mapGuideSize.z));
            GetMapGuidePose(out Vector3 guidePosition, out Quaternion guideRotation);

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = Matrix4x4.TRS(guidePosition, guideRotation, Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up * (guideSize.y * 0.5f), guideSize);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }

        private void GetMapGuidePose(out Vector3 position, out Quaternion rotation)
        {
            if (mapGuideCenter != null)
            {
                position = mapGuideCenter.position;
                rotation = mapGuideCenter.rotation;
                return;
            }

            if (TryGetFloorBounds(out Bounds floorBounds))
            {
                position = new Vector3(floorBounds.center.x, floorBounds.max.y, floorBounds.center.z);
                rotation = floor.rotation;
                return;
            }

            position = transform.position;
            rotation = transform.rotation;
        }

        private bool TryGetFloorBounds(out Bounds bounds)
        {
            bounds = default;

            if (floor == null)
                return false;

            Renderer[] renderers = floor.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                return true;
            }

            Collider[] colliders = floor.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
                return false;

            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return true;
        }

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
