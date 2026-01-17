using Enemy;
using UnityEditor.Rendering;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] float _rotateSpeed = 40f;
    [SerializeField] int orbAtk = 1;
    float rotateSpeed;
    private Transform rotatePivot;

    void Update()
    {
        Rotate();
    }

    public void InitilaizeOrb(Transform _pivot, bool clockwise) 
    {
        rotatePivot = _pivot;
        rotateSpeed = clockwise ? _rotateSpeed : -_rotateSpeed;
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
            damagable.TakeDamage(orbAtk);
        }
    }
}
