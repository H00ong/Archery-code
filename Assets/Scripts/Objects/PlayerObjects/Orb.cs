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
        // Rigidbody 소유자(있으면 그쪽, 없으면 collider 자신)
        var hitRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject
                                              : other.gameObject;

        // 가장 흔한 판정 패턴
        if (hitRoot.TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.GetHit(orbAtk);
        }
    }
}
