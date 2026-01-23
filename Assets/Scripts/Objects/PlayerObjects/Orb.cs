using Enemy;
using Players;
using UnityEditor.Rendering;
using UnityEngine;

public readonly struct OrbInitConfig
{
    public readonly Transform Pivot;
    public readonly bool Clockwise;
    public readonly DamageInfo DamageInfo;

    public OrbInitConfig(Transform pivot, bool clockwise, DamageInfo damageInfo)
    {
        Pivot = pivot;
        Clockwise = clockwise;
        DamageInfo = damageInfo;
    }
}

public class Orb : MonoBehaviour
{
    [SerializeField] float defaultRotateSpeed = 40f;
    
    private DamageInfo _damageInfo;
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
        _damageInfo = config.DamageInfo;
        _damageInfo.attackSource = gameObject;
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
            damagable.TakeDamage(_damageInfo);
            Debug.Log($"[Orb] Hit {other.gameObject.name} | Type: {_damageInfo.type} | Damage: {_damageInfo.damageAmount}");
        }
    }
}
