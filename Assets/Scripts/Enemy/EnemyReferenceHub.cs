using System.Collections.Generic;
using Map;
using Managers;
using UnityEngine;

namespace Enemy
{
    public class EnemyReferenceHub : MonoBehaviour
    {
        // [인스펙터용 구조체] Enum과 실제 Transform을 짝지어주는 역할
        [System.Serializable]
        public struct PointData
        {
            public EnemyPointType type;
            public List<Transform> points;
        }

        [Header("Configuration")]
        [Tooltip("인스펙터에서 Enum과 실제 Transform을 연결해주세요.")]
        [SerializeField] private List<PointData> pointGroups;
        private Dictionary<EnemyPointType, List<Transform>> _pointDict;
        private List<Vector3> _patrolPoints;

        public void Init()
        {
            _pointDict = new Dictionary<EnemyPointType, List<Transform>>();

            foreach (var group in pointGroups)
            {
                if (!_pointDict.ContainsKey(group.type))
                    _pointDict.Add(group.type, group.points);
            }
        }

        public List<Transform> GetPoints(EnemyPointType type)
        {
            if (_pointDict != null && _pointDict.TryGetValue(type, out var list))
                return list;

            return null;
        }

        public List<Vector3> GetPatrolPoints()
        {
            if (_patrolPoints is not null && _patrolPoints.Count > 0)
                return _patrolPoints;
            
            var allPatrolPoints = MapManager.Instance.GetAllPatrolPoints();

            if (allPatrolPoints != null && allPatrolPoints.Count > 0)
            {
                PatrolPoint nearest = allPatrolPoints[0];
                nearest = FindNearestPatrolPoint(allPatrolPoints, nearest);

                _patrolPoints = nearest.GetPatrolPositions();
            }
            
            return _patrolPoints;
        }

        private PatrolPoint FindNearestPatrolPoint(List<PatrolPoint> allPatrolPoints, PatrolPoint nearest)
        {
            float minDist = Vector3.Distance(transform.position, nearest.transform.position);

            for (int i = 1; i < allPatrolPoints.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, allPatrolPoints[i].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = allPatrolPoints[i];
                }
            }

            return nearest;
        }

        void OnDisable()
        {
            _pointDict?.Clear();
            _patrolPoints?.Clear();
        }
    }
}