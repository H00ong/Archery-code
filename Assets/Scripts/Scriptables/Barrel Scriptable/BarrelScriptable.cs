using Game.Player;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "BarrelScriptable", menuName = "Scriptable Objects/BarrelScriptable")]
public class BarrelScriptable : ScriptableObject
{
    public BarrelType type;
    public AssetReferenceGameObject barrelPrefab;
    public AssetReferenceGameObject meteorPrefab;
}
