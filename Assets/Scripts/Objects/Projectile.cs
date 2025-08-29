using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected PoolManager poolManager;
    protected Rigidbody rb;
    protected Collider cd;

    protected float lifetimeTimer = 0f;
    protected float lifetime;
    protected bool spawned = false;

    public int damage { get; protected set; } = 1;


    protected virtual void Start()
    {
        cd = GetComponentInChildren<Collider>();
        cd.isTrigger = true;
    }

    protected virtual void Update()
    {
        if (spawned) 
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
        spawned = true;

        gameObject.SetActive(true);

        rb = GetComponent<Rigidbody>();
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();

        transform.position = _pos;
        transform.rotation = Quaternion.LookRotation(_dir);
        rb.linearVelocity = transform.forward * _speed;
        damage = _damage;
        lifetime = _lifetime;
    }

    protected void Terminate()
    {
        spawned = false;
        lifetimeTimer = 0f;
        poolManager.ReturnObject(gameObject);
    }

}
