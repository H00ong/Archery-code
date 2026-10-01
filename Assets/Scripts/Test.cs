using System.Collections.Generic;
using System.Linq; // ★ LINQ 필수!
using UnityEngine; // ★ Unity 필수

public class Test : MonoBehaviour
{
    [ContextMenu("Remove Child Colliders")]
    public void RemoveCollidersFromChildren()
    {
        // 비활성화된 자식 객체(true)까지 포함하여 모든 3D 콜라이더를 가져옵니다.
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        int removedCount = 0;

        foreach (Collider col in allColliders)
        {
            // 부모 객체(이 스크립트가 붙은 객체)의 콜라이더는 유지합니다.
            if (col.gameObject != this.gameObject)
            {
                // 에디터(편집 모드)에서 컴포넌트를 삭제할 때는 DestroyImmediate를 사용합니다.
                DestroyImmediate(col);
                removedCount++;
            }
        }
        
        Debug.Log($"자식 객체들의 콜라이더가 총 {removedCount}개 제거되었습니다.");
    }
}
