using UnityEngine;

// 이거는 맵 transform 설정을 하기 위한 스크립트   
public class MapSetter : MonoBehaviour
{
    private void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.position = new Vector3(child.position.x - 2.71f * i, child.position.y, child.position.z);
            child.gameObject.layer = LayerMask.NameToLayer("Obstacle");
        }
    }
}
