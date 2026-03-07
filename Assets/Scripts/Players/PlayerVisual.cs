using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Material characterMaterial;
    [SerializeField] private Material weaponMaterial;

    [SerializeField] List<MeshRenderer> characterMeshRenderers;
    [SerializeField] List<MeshRenderer> weaponMeshRenderers;

    [ContextMenu("Find Mesh Renderers")]
    private void FindMeshRenderers()
    {
        characterMeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        weaponMeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
    }


}
