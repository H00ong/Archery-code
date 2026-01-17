using System.Collections.Generic;
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

        public void Init()
        {
            _pointDict = new Dictionary<EnemyPointType, List<Transform>>();
            
            foreach (var group in pointGroups)
            {
                if (!_pointDict.ContainsKey(group.type))
                {
                    _pointDict.Add(group.type, group.points);
                }
            }
        }

        public List<Transform> GetPoints(EnemyPointType type)
        {
            if (_pointDict != null && _pointDict.TryGetValue(type, out var list))
            {
                return list;
            }

            return null;
        }
    }
}