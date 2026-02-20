using Players;
using Stat;
using UnityEngine;

public readonly struct OrbInitConfig
{
    public readonly Transform pivot;
    public readonly float rotateSpeed;
    public readonly EffectType effectType;
    public readonly float damageModifier;

    public OrbInitConfig(Transform pivot, float rotateSpeed, EffectType effectType, float damageModifier)
    {
        this.pivot = pivot;
        this.rotateSpeed = rotateSpeed;
        this.effectType = effectType;
        this.damageModifier = damageModifier;
    }
}

public class Orb : MonoBehaviour
{
    private float _rotateSpeed;
    private Transform _rotatePivot;
    private EffectType _effectType;
    private float _damageModifier;

    void Update()
    {
        Rotate();
    }

    public void InitializeOrb(OrbInitConfig config)
    {
        _rotatePivot = config.pivot;
        _rotateSpeed = config.rotateSpeed;
        _effectType = config.effectType;
        _damageModifier = config.damageModifier;
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
            var stat = PlayerController.Instance.Stat;
            float damage = stat.AttackPower * _damageModifier;
            var damageInfo = new DamageInfo(damage, _effectType, stat, gameObject);

            damagable.TakeDamage(damageInfo);
            Debug.Log($"[Orb] Hit {other.gameObject.name} | Type: {damageInfo.type} | Damage: {damageInfo.damageAmount}");
        }
    }
}
