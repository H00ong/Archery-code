using Game.Player;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "BarrelScriptable", menuName = "BarrelData")]
public class BarrelScriptable : ScriptableObject
{
    public BarrelType type;
    public AssetReferenceGameObject barrelPrefab;
    public AssetReferenceGameObject meteorPrefab;
}
