using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Material characterMaterial;
    [SerializeField] private Material weaponMaterial;

    // MeshRenderer/SkinnedMeshRenderer 공용 베이스 타입(Renderer)으로 통일해서 둘 다 담을 수 있도록 함
    [SerializeField] private List<Renderer> characterMeshRenderers;
    [SerializeField] private List<Renderer> weaponMeshRenderers;

    [Header("Effect Colors")]
    [SerializeField] private Color fireEmissionColor      = new Color(53f / 255f,  9f / 255f,  0f / 255f, 1f);
    [SerializeField] private Color poisonEmissionColor    = new Color(15f / 255f,  0f / 255f, 39f / 255f, 1f);
    [SerializeField] private Color iceEmissionColor       = new Color( 0f / 255f, 16f / 255f, 39f / 255f, 1f);
    [SerializeField] private Color lightningEmissionColor = new Color(24f / 255f, 15f / 255f,  0f / 255f, 1f);
    [SerializeField] private Color magmaEmissionColor     = new Color(39f / 255f, 12f / 255f,  1f / 255f, 1f);
    [SerializeField] private Color darkEmissionColor      = new Color(24f / 255f, 24f / 255f, 24f / 255f, 1f);
    [SerializeField] private float intensity = 1f;

    private Health _health;
    private Color _originalEmissionColor = Color.black;
    private bool _useEmission = false;
    private Dictionary<EffectType, Color> _effectColorMap;

    private void Awake()
    {
        _effectColorMap = new Dictionary<EffectType, Color>
        {
            { EffectType.Fire,      fireEmissionColor      },
            { EffectType.Poison,    poisonEmissionColor    },
            { EffectType.Ice,       iceEmissionColor       },
            { EffectType.Lightning, lightningEmissionColor },
            { EffectType.Magma,     magmaEmissionColor     },
            { EffectType.Dark,      darkEmissionColor      },
        };
    }

    private void Start()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            Debug.LogWarning("[PlayerVisual] Health component not found.");
            return;
        }

        _health.OnStatusChanged -= VisualizeEffect;
        _health.OnStatusChanged += VisualizeEffect;

        CacheOriginalEmissionColors();

        EventBus.Subscribe(EventType.Retry, ResetVisual);
        EventBus.Subscribe(EventType.TransitionToLobby, ResetVisual);
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnStatusChanged -= VisualizeEffect;

        EventBus.Unsubscribe(EventType.Retry, ResetVisual);
        EventBus.Unsubscribe(EventType.TransitionToLobby, ResetVisual);
    }

    private void CacheOriginalEmissionColors()
    {
        if (characterMeshRenderers == null || characterMeshRenderers.Count == 0) return;

        var mat = characterMeshRenderers[0] != null ? characterMeshRenderers[0].material : null;
        if (mat == null) mat = characterMaterial;

        if (mat != null && mat.HasProperty(EmissionColor))
        {
            _originalEmissionColor = mat.GetColor(EmissionColor);
            _useEmission = mat.IsKeywordEnabled("_EMISSION");
        }
    }

    private void VisualizeEffect(DamageInfo damageInfo, bool isStart)
    {
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
        foreach (var renderer in characterMeshRenderers)
        {
            if (renderer == null) continue;
            if (!_useEmission)
                renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor(EmissionColor, color * intensity);
        }
    }

    private void RestoreOriginalEmissionColors()
    {
        foreach (var renderer in characterMeshRenderers)
        {
            if (renderer == null) continue;
            if (!_useEmission)
                renderer.material.DisableKeyword("_EMISSION");
            renderer.material.SetColor(EmissionColor, _originalEmissionColor);
        }
    }

    private void ResetVisual()
    {
        RestoreOriginalEmissionColors();
    }

    [ContextMenu("Find Mesh Renderers (Mesh + Skinned)")]
    private void FindMeshRenderers()
    {
        characterMeshRenderers = CollectMeshAndSkinnedRenderers(transform);
        weaponMeshRenderers = CollectMeshAndSkinnedRenderers(transform);
    }

    [ContextMenu("Find Skinned Mesh Renderers Only")]
    private void FindSkinnedMeshRenderers()
    {
        characterMeshRenderers = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        weaponMeshRenderers = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>(true));
    }

    /// <summary> MeshRenderer와 SkinnedMeshRenderer를 모두 찾아 하나의 Renderer 리스트로 합친다. </summary>
    private static List<Renderer> CollectMeshAndSkinnedRenderers(Transform root)
    {
        var result = new List<Renderer>();
        result.AddRange(root.GetComponentsInChildren<MeshRenderer>(true));
        result.AddRange(root.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        return result;
    }
}
