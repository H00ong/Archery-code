using System.Collections.Generic;
using UnityEngine;

public class LobbyMapSetter : MonoBehaviour
{
    [Header("Map Settings")]
    public float yOffset = 30f;
    public List<GameObject> maps = new List<GameObject>();

    [ContextMenu("Arrange Map Y Offset")]
    public void ArrangeMaps()
    {
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i] == null) continue;

            Vector3 pos = maps[i].transform.position;
            maps[i].transform.position = new Vector3(pos.x, i * yOffset, pos.z);
        }
    }
}
