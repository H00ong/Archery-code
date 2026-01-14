using Game.Player;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "OrbData", menuName = "OrbData")]
public class OrbScriptable : ScriptableObject
{
    public OrbType orbType;
    public AssetReferenceGameObject orb;
}
