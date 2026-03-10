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
        [SerializeField] private Color fireEmissionColor = new Color(53f / 255f, 9f / 255f, 0f / 255f, 1f);
        [SerializeField] private Color poisonEmissionColor = new Color(15f / 255f, 0f / 255f, 39f / 255f, 1f);
        [SerializeField] private Color iceEmissionColor = new Color(0f / 255f, 16f / 255f, 39f / 255f, 1f);
        [SerializeField] private Color lightningEmissionColor = new Color(24f / 255f, 15f / 255f, 0f / 255f, 1f);
        [SerializeField] private Color magmaEmissionColor = new Color(39f / 255f, 12f / 255f, 1f / 255f, 1f);
        [SerializeField] private Color darkEmissionColor = new Color(24f / 255f, 24f / 255f, 24f / 255f, 1f);
        [SerializeField] private float intensity = 1f;

        private EnemyController _enemyController;
        private Health _health;
        private Color _originalEmissionColor = Color.black;
        private bool _useEmission = false;

        // 효과 타입별 색상 매핑
        private Dictionary<EffectType, Color> _effectColorMap;

        private void Awake()
        {
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
            if (material == null)
                return;

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
            if (material == null)
                return;

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
            _enemyController = GetComponent<EnemyController>();
            _health = _enemyController.GetComponent<Health>();

            _health.OnStatusChanged -= VisualizeEffect;
            _health.OnStatusChanged += VisualizeEffect;

            RestoreOriginalEmissionColors();

            if (_health == null)
            {
                Debug.LogWarning("Health component not found on EnemyController.");
                return;
            }
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

                    renderer.material.SetColor(EmissionColor, color * intensity);
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