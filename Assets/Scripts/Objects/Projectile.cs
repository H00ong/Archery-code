using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Rigidbody rb;
    protected Collider cd;

    protected float lifetimeTimer = 0f;
    protected float lifetime;

    public int damage { get; protected set; } = 1;


    protected virtual void Start()
    {
        cd = GetComponentInChildren<Collider>();
        cd.isTrigger = true;
    }

    protected virtual void Update()
    {
        if (gameObject.activeSelf) 
        {
            lifetimeTimer += Time.deltaTime;

            if (lifetimeTimer >= lifetime)
            {
                Terminate();
            }
        }
    }

    public virtual void SetupProjectile(Vector3 _pos, Vector3 _dir, float _speed, int _damage, float _lifetime)
    {
        rb = GetComponent<Rigidbody>();

        transform.position = _pos;
        transform.rotation = Quaternion.LookRotation(_dir);
        rb.linearVelocity = transform.forward * _speed;
        damage = _damage;
        lifetime = _lifetime;
    }

    protected void Terminate()
    {
        lifetimeTimer = 0f;
        GameManager.Instance.StageManager.PoolManager.ReturnObject(gameObject);
    }
}
