using System;
using Map;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonMap", menuName = "MapData/DungeonMap")]
public class DungeonMap : MapScriptable
{
    private void OnValidate()
    {
        mapType = MapType.Dungeon;
    }
}
