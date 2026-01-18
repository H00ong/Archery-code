using Enemy;
using Players;
using UnityEditor.Rendering;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] float _rotateSpeed = 40f;
    [SerializeField] int orbAtk = 1;
    [SerializeField] OrbType orbType = OrbType.Common;
    
    float rotateSpeed;
    private Transform rotatePivot;

    void Update()
    {
        Rotate();
    }

    public void InitilaizeOrb(Transform _pivot, bool clockwise, OrbType type = OrbType.Common) 
    {
        rotatePivot = _pivot;
        rotateSpeed = clockwise ? _rotateSpeed : -_rotateSpeed;
        orbType = type;
    }

    private void Rotate() 
    {
        transform.RotateAround(rotatePivot.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.GetComponentInParent<IDamageable>();

        if (damagable != null) 
        {
            DamageType damageType = ConvertOrbTypeToDamageType(orbType);
            var damageInfo = new DamageInfo(orbAtk, damageType, gameObject);
            damageInfo.hitPoint = other.ClosestPoint(transform.position);
            damagable.TakeDamage(damageInfo);
        }
    }

    private DamageType ConvertOrbTypeToDamageType(OrbType type)
    {
        return type switch
        {
            OrbType.Venom => DamageType.Venom,
            OrbType.Blaze => DamageType.Fire,
            OrbType.Ice => DamageType.Ice,
            _ => DamageType.Normal
        };
    }
}
