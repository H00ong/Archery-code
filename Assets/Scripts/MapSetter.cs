using UnityEngine;

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
