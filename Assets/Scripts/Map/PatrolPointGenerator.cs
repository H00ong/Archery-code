using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class PatrolPoint : MonoBehaviour
    {
        public enum ShapeType { Rectangle, Rhombus, Triangle }

        [Header("Shape Settings")]
        [Tooltip("도형의 형태를 선택합니다.")]
        [SerializeField] private ShapeType shapeType = ShapeType.Rectangle;
        [Tooltip("도형의 가로 너비 (마름모는 가로 대각선, 삼각형은 밑변)")]
        [SerializeField] private float width = 5f;
        [Tooltip("도형의 세로 길이 (마름모는 세로 대각선, 삼각형은 높이)")]
        [SerializeField] private float height = 5f;

        [Header("Point Settings")]
        [Tooltip("생성할 패트롤 포인트의 개수")]
        [SerializeField] private int pointCount = 4;

        /// <summary>
        /// 설정된 도형의 둘레를 따라 균등하게 배치된 월드 좌표 목록을 반환합니다.
        /// </summary>
        public List<Vector3> GetPatrolPositions()
        {
            var positions = new List<Vector3>();

            if (pointCount <= 0)
                return positions;

            float totalPerimeter = GetTotalPerimeter();
            float stepDistance = totalPerimeter / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float currentDistance = stepDistance * i;
                Vector3 localPos = GetPointAtDistance(currentDistance);
                positions.Add(transform.position + transform.rotation * localPos);
            }

            return positions;
        }

        private float GetTotalPerimeter()
        {
            if (shapeType == ShapeType.Rectangle)
            {
                return (width * 2) + (height * 2);
            }
            else if (shapeType == ShapeType.Rhombus)
            {
                float sideLength = Mathf.Sqrt(Mathf.Pow(width / 2f, 2) + Mathf.Pow(height / 2f, 2));
                return sideLength * 4;
            }
            else // Triangle
            {
                // 피타고라스 정리로 빗변의 길이 계산
                float sideLength = Mathf.Sqrt(Mathf.Pow(width / 2f, 2) + Mathf.Pow(height, 2));
                // 밑변 + (빗변 * 2)
                return width + (sideLength * 2);
            }
        }

        private Vector3 GetPointAtDistance(float distance)
        {
            float halfW = width / 2f;
            float halfH = height / 2f;

            if (shapeType == ShapeType.Rectangle)
            {
                float topEdge = width;
                float rightEdge = width + height;
                float bottomEdge = (width * 2) + height;

                if (distance <= topEdge)
                {
                    float t = distance / topEdge;
                    return Vector3.Lerp(new Vector3(-halfW, 0, halfH), new Vector3(halfW, 0, halfH), t);
                }
                else if (distance <= rightEdge)
                {
                    float t = (distance - topEdge) / height;
                    return Vector3.Lerp(new Vector3(halfW, 0, halfH), new Vector3(halfW, 0, -halfH), t);
                }
                else if (distance <= bottomEdge)
                {
                    float t = (distance - rightEdge) / width;
                    return Vector3.Lerp(new Vector3(halfW, 0, -halfH), new Vector3(-halfW, 0, -halfH), t);
                }
                else
                {
                    float t = (distance - bottomEdge) / height;
                    return Vector3.Lerp(new Vector3(-halfW, 0, -halfH), new Vector3(-halfW, 0, halfH), t);
                }
            }
            else if (shapeType == ShapeType.Rhombus)
            {
                float sideLength = GetTotalPerimeter() / 4f;

                Vector3 topNode = new Vector3(0, 0, halfH);
                Vector3 rightNode = new Vector3(halfW, 0, 0);
                Vector3 bottomNode = new Vector3(0, 0, -halfH);
                Vector3 leftNode = new Vector3(-halfW, 0, 0);

                if (distance <= sideLength)
                {
                    float t = distance / sideLength;
                    return Vector3.Lerp(topNode, rightNode, t);
                }
                else if (distance <= sideLength * 2)
                {
                    float t = (distance - sideLength) / sideLength;
                    return Vector3.Lerp(rightNode, bottomNode, t);
                }
                else if (distance <= sideLength * 3)
                {
                    float t = (distance - sideLength * 2) / sideLength;
                    return Vector3.Lerp(bottomNode, leftNode, t);
                }
                else
                {
                    float t = (distance - sideLength * 3) / sideLength;
                    return Vector3.Lerp(leftNode, topNode, t);
                }
            }
            else // Triangle
            {
                float sideLength = Mathf.Sqrt(Mathf.Pow(width / 2f, 2) + Mathf.Pow(height, 2));
                
                // 삼각형의 세 꼭짓점 (좌하단 -> 상단 -> 우하단)
                Vector3 bottomLeftNode = new Vector3(-halfW, 0, -halfH);
                Vector3 topNode = new Vector3(0, 0, halfH);
                Vector3 bottomRightNode = new Vector3(halfW, 0, -halfH);

                // 좌하단에서 시작해 시계방향으로 이동
                if (distance <= sideLength)
                {
                    float t = distance / sideLength;
                    return Vector3.Lerp(bottomLeftNode, topNode, t);
                }
                else if (distance <= sideLength * 2)
                {
                    float t = (distance - sideLength) / sideLength;
                    return Vector3.Lerp(topNode, bottomRightNode, t);
                }
                else
                {
                    float t = (distance - sideLength * 2) / width;
                    return Vector3.Lerp(bottomRightNode, bottomLeftNode, t);
                }
            }
        }

        // 씬 뷰(Scene View)에서 도형의 가이드라인을 초록색 선으로 보여줍니다.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            float halfW = width / 2f;
            float halfH = height / 2f;

            if (shapeType == ShapeType.Rectangle)
            {
                Vector3 tl = center + new Vector3(-halfW, 0, halfH);
                Vector3 tr = center + new Vector3(halfW, 0, halfH);
                Vector3 br = center + new Vector3(halfW, 0, -halfH);
                Vector3 bl = center + new Vector3(-halfW, 0, -halfH);

                Gizmos.DrawLine(tl, tr);
                Gizmos.DrawLine(tr, br);
                Gizmos.DrawLine(br, bl);
                Gizmos.DrawLine(bl, tl);
            }
            else if (shapeType == ShapeType.Rhombus)
            {
                Vector3 t = center + new Vector3(0, 0, halfH);
                Vector3 r = center + new Vector3(halfW, 0, 0);
                Vector3 b = center + new Vector3(0, 0, -halfH);
                Vector3 l = center + new Vector3(-halfW, 0, 0);

                Gizmos.DrawLine(t, r);
                Gizmos.DrawLine(r, b);
                Gizmos.DrawLine(b, l);
                Gizmos.DrawLine(l, t);
            }
            else // Triangle
            {
                Vector3 bl = center + new Vector3(-halfW, 0, -halfH);
                Vector3 t = center + new Vector3(0, 0, halfH);
                Vector3 br = center + new Vector3(halfW, 0, -halfH);

                Gizmos.DrawLine(bl, t);
                Gizmos.DrawLine(t, br);
                Gizmos.DrawLine(br, bl);
            }
        }
    }
}