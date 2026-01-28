using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyVisual : MonoBehaviour
    {
        [Header("Object Renderers")]
        [SerializeField] private List<Renderer> objectRenderers = new List<Renderer>();
        
        [Header("Accessory Renderers")]
        [SerializeField] private List<Renderer> accessoryRenderers = new List<Renderer>();

        /// <summary>
        /// 모든 오브젝트 렌더러에 메인 머티리얼 적용
        /// </summary>
        public void ApplyObjectMaterial(Material material)
        {
            if (material == null) return;
            
            foreach (var renderer in objectRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        /// <summary>
        /// 모든 악세사리 렌더러에 악세사리 머티리얼 적용
        /// </summary>
        public void ApplyAccessoryMaterial(Material material)
        {
            if (material == null) return;
            
            foreach (var renderer in accessoryRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        /// <summary>
        /// EnemyIdentity의 머티리얼들을 한번에 적용
        /// </summary>
        public void ApplyMaterials(Material objectMat, Material accessoryMat)
        {
            ApplyObjectMaterial(objectMat);
            ApplyAccessoryMaterial(accessoryMat);
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Find Renderers")]
        private void AutoFindRenderers()
        {
            objectRenderers.Clear();
            accessoryRenderers.Clear();
            
            var allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in allRenderers)
            {
                // 기본적으로 objectRenderers에 추가, 필요시 수동으로 accessoryRenderers로 이동
                objectRenderers.Add(renderer);
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
