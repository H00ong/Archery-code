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
            public EnemyPointType type;   // 예: FlyingMuzzle
            public List<Transform> points;   // 예: RightHand_Point (실제 오브젝트)
        }

        [Header("Configuration")]
        [Tooltip("인스펙터에서 Enum과 실제 Transform을 연결해주세요.")]
        // 1. [필수 프로퍼티] 인스펙터 설정용 리스트
        [SerializeField] private List<PointData> pointGroups; 
        
        // 2. [필수 프로퍼티] 런타임 검색용 딕셔너리 (외부에서는 몰라도 됨)
        private Dictionary<EnemyPointType, List<Transform>> _pointDict;

        /// <summary>
        /// EnemyController의 InitializeEnemy 단계에서 호출되어 딕셔너리를 구축합니다.
        /// </summary>
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
            return new List<Transform>(); // 없으면 빈 리스트 반환 (null 아님)
        }
    }
}