using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyVisual : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [Header("Object Renderers")]
        [SerializeField] private List<Renderer> objectRenderers = new List<Renderer>();
        
        [Header("Accessory Renderers")]
        [SerializeField] private List<Renderer> accessoryRenderers = new List<Renderer>();

        [Header("Effect Colors")]
        [SerializeField] private Color fireEmissionColor = new Color(1f, 0.3f, 0f, 1f) * 2f;  // 붉은 발광
        [SerializeField] private Color poisonEmissionColor = new Color(0.5f, 0f, 0.8f, 1f) * 1.5f;  // 보라색 발광
        [SerializeField] private Color iceEmissionColor = new Color(0.3f, 0.8f, 1f, 1f) * 1.5f;  // 차가운 하늘색 발광
        [SerializeField] private Color lightningEmissionColor = new Color(1f, 1f, 0.2f, 1f) * 2.5f;  // 밝은 노란색 발광
        [SerializeField] private Color magmaEmissionColor = new Color(1f, 0.5f, 0f, 1f) * 3f;  // 뜨거운 주황색 발광
        [SerializeField] private Color darkEmissionColor = new Color(0.2f, 0f, 0.3f, 1f) * 1.5f;  // 어두운 보라색 발광

        private EnemyController _enemyController;
        private Color _originalEmissionColor = Color.black;
        private bool _useEmission = false;
        
        // 효과 타입별 색상 매핑
        private Dictionary<EffectType, Color> _effectColorMap;

        private void Awake()
        {
            _enemyController = GetComponent<EnemyController>();
            
            // 효과 색상 매핑 초기화
            _effectColorMap = new Dictionary<EffectType, Color>
            {
                { EffectType.Fire, fireEmissionColor },
                { EffectType.Poison, poisonEmissionColor },
                { EffectType.Ice, iceEmissionColor },
                { EffectType.Lightning, lightningEmissionColor },
                { EffectType.Magma, magmaEmissionColor },
                { EffectType.Dark, darkEmissionColor }
            };
        }

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

            // 새 Material 적용 후 원래 색상 저장
            CacheOriginalEmissionColors(material);
        }

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

        public void Initialize()
        {
            Health health = _enemyController.GetComponent<Health>();

            if (health == null)
            {
                Debug.LogWarning("Health component not found on EnemyController.");
                return;
            }

            health.OnStatusChanged += VisualizeEffect;
        }

        private void CacheOriginalEmissionColors(Material material)
        {
            if (material.HasProperty(EmissionColor))
            {
                _originalEmissionColor = material.GetColor(EmissionColor);
                _useEmission = material.IsKeywordEnabled("_EMISSION");
            }
        }

        private void VisualizeEffect(DamageInfo damageInfo, bool isStart)
        {
            // 모든 효과 타입을 순회하며 체크
            foreach (var kvp in _effectColorMap)
            {
                if (Utils.HasEffectType(damageInfo.type, kvp.Key))
                {
                    if (isStart)
                        SetEmissionColor(kvp.Value);
                    else
                        RestoreOriginalEmissionColors();
                }
            }
        }

        private void SetEmissionColor(Color color)
        {
            foreach (var renderer in objectRenderers)
            {
                if (renderer != null)
                {
                    if (!_useEmission)
                    {
                        renderer.material.EnableKeyword("_EMISSION");
                    }

                    renderer.material.SetColor(EmissionColor, color);
                }
            }
        }

        private void RestoreOriginalEmissionColors()
        {
            foreach (var renderer in objectRenderers)
            {
                if (renderer != null)
                {
                    if (!_useEmission)
                    {
                        renderer.material.DisableKeyword("_EMISSION");
                    }

                    renderer.material.SetColor(EmissionColor, _originalEmissionColor);
                }
            }
        }

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
                objectRenderers.Add(renderer);
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}