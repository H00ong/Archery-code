using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MapData", menuName = "MapData")]
public abstract class MapScriptable : ScriptableObject
{
    public MapType mapType;
    public List<AssetReferenceGameObject> mapList;
    public List<AssetReferenceGameObject> enemyList;
    public List<AssetReferenceGameObject> bossList;
    public AssetReferenceGameObject bossMap;
}
