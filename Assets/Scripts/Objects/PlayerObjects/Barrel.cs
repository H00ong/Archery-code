using System.Collections;
using Managers;
using UnityEngine;
using Objects;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Barrel : SceneObject
{
    [SerializeField] float _lifeTime = 20f;
    [SerializeField] EffectType _type;
    private BarrelManager _barrelManager;
    private bool _isTouched = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe(EventType.StageCleared, OnStageCleared);
        StartCoroutine(TerminateCoroutine());
    }

    protected override void OnDisable()
    {
        EventBus.Unsubscribe(EventType.StageCleared, OnStageCleared);
        base.OnDisable();
    }

    private void OnStageCleared()
    {
        StopAllCoroutines();
        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void Start()
    {
        _barrelManager = BarrelManager.Instance;
    }

    public void InitBarrel(EffectType type)
    {
        _type = type;
        _isTouched = false;
    }

    IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(_lifeTime);

        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (root.CompareTag(Utils.TagMap[TagType.Player]) && !_isTouched)
        {
            _isTouched = true;

            BarrelManager.Instance.OnBarrelPickedUp(_type);

            StopAllCoroutines();
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Particle Color/Yellow")]
    private void SetParticleColorYellow() => SetParticleStartColor(Color.yellow, "Set barrel particles to yellow");

    [ContextMenu("Particle Color/Cyan")]
    private void SetParticleColorCyan() => SetParticleStartColor(new Color(0.21f, 0.79f, 0.91f), "Set barrel particles to cyan");

    [ContextMenu("Particle Color/White")]
    private void SetParticleColorWhite() => SetParticleStartColor(Color.white, "Set barrel particles to white");

    [ContextMenu("Particle Color/Green")]
    private void SetParticleColorGreen() => SetParticleStartColor(new Color(0.22f, 0.79f, 0.42f), "Set barrel particles to green");

    [ContextMenu("Particle Color/Purple")]
    private void SetParticleColorPurple() => SetParticleStartColor(new Color(0.65f, 0.2f, 1f), "Set barrel particles to purple");

    [ContextMenu("Particle Color/Red")]
    private void SetParticleColorRed() => SetParticleStartColor(Color.red, "Set barrel particles to red");

    private void SetParticleStartColor(Color color, string undoName)
    {
        var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        Undo.RecordObjects(particleSystems, undoName);

        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            main.startColor = color;
            EditorUtility.SetDirty(particleSystem);
        }
    }
#endif
}
