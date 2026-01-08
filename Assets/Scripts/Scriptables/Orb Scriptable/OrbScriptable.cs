using Game.Player;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "OrbScriptable", menuName = "Scriptable Objects/Orb")]
public class OrbScriptable : ScriptableObject
{
    public OrbType orbType;
    public AssetReferenceGameObject orb;
}
