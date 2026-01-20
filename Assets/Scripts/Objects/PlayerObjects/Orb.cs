using Enemy;
using Players;
using UnityEditor.Rendering;
using UnityEngine;

public readonly struct OrbInitConfig
{
    public readonly Transform Pivot;
    public readonly bool Clockwise;
    public readonly OrbType Type;
    public readonly int Damage;

    public OrbInitConfig(Transform pivot, bool clockwise, OrbType type, int damage)
    {
        Pivot = pivot;
        Clockwise = clockwise;
        Type = type;
        Damage = damage;
    }
}

public class Orb : MonoBehaviour
{
    [SerializeField] float defaultRotateSpeed = 40f;
    
    private int _damage;
    private OrbType _orbType;
    private float _rotateSpeed;
    private Transform _rotatePivot;

    void Update()
    {
        Rotate();
    }

    public void Initialize(OrbInitConfig config) 
    {
        _rotatePivot = config.Pivot;
        _rotateSpeed = config.Clockwise ? defaultRotateSpeed : -defaultRotateSpeed;
        _orbType = config.Type;
        _damage = config.Damage;
    }

    private void Rotate() 
    {
        transform.RotateAround(_rotatePivot.position, Vector3.up, _rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.GetComponentInParent<IDamageable>();

        if (damagable != null) 
        {
            EffectType damageType = ConvertOrbTypeToDamageType(_orbType);
            var damageInfo = new DamageInfo(_damage, damageType, gameObject);
            damageInfo.hitPoint = other.ClosestPoint(transform.position);
            damagable.TakeDamage(damageInfo);
            Debug.Log($"[Orb] Hit {other.gameObject.name} | Type: {_orbType} | Damage: {_damage}");
        }
    }

    private EffectType ConvertOrbTypeToDamageType(OrbType type)
    {
        return type switch
        {
            OrbType.Venom => EffectType.Venom,
            OrbType.Blaze => EffectType.Fire,
            OrbType.Ice => EffectType.Ice,
            _ => EffectType.Normal
        };
    }
}
