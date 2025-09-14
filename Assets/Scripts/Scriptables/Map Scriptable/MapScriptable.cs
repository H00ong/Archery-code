using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Scriptable", menuName = "Scriptable Objects/Scriptable")]
public abstract class MapScriptable : ScriptableObject
{
    public MapType mapType;
    public List<AssetReferenceGameObject> mapList;
    public List<AssetReferenceGameObject> enemyList;
    public List<AssetReferenceGameObject> bossList;
    public AssetReferenceGameObject bossMap;
}
