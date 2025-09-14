using System.Collections;
using UnityEngine;

public readonly struct ShootRequest
{
    public readonly Vector3 positionWS;
    public readonly Vector3 directionWS;   // normalized
    public readonly float speed;
    public readonly float lifetime;
    public readonly DamageInfo damage;

    public ShootRequest(Vector3 pos, Vector3 dir, float speed, float lifetime, in DamageInfo dmg)
    {
        positionWS = pos;
        directionWS = dir.normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        damage = dmg;
    }
}

public readonly struct DamageInfo
{
    public readonly int baseDamage;
    public readonly float critChance, critMultiplier;

    public readonly float poisonDPS, poisonDuration;
    public readonly float burnDPS, burnDuration;

    public readonly float slowPct, slowDuration;

    public readonly int sourceId;   // 공격자 식별(아군/적 판정, 킬 크레딧)
    public readonly int elementMask;
    public readonly uint attackSeq; // 재현성/로그 구분
}

public class Projectile : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] protected PoolManager poolManager;
    [Header("Identity")]
    [SerializeField] protected bool hasExplosionEffect = false;

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

    public virtual void SetupProjectile(Vector3 _pos, Vector3 _dir, float _speed, int _damage, float _lifetime, bool _isFlying)
    {
        spawned = true;

        rb = GetComponent<Rigidbody>();
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();

        transform.position = _pos; 
        transform.rotation = Quaternion.LookRotation(_dir);

        if (_isFlying)
        {
            rb.linearVelocity = _dir;
            rb.useGravity = true;
        }
        else
        {
            rb.linearVelocity = _dir * _speed;
        }

        damage = _damage;
        lifetime = _lifetime;
    }

    protected void Terminate()
    {
        spawned = false;
        rb.useGravity = false;
        lifetimeTimer = 0f;
        poolManager.ReturnObject(gameObject);
    }
}
