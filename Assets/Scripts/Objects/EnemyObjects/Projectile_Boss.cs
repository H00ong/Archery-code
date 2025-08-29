using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(Rigidbody))]
public class Projectile_Boss : Projectile_Enemy
{
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject explosionEffect;
    [SerializeField] Transform effectParent;
    bool floorHit = false;

    protected override void Start()
    {
        base.Start();

        gameObject.tag = enemyProjectileTag;
    }

    protected override void Update()
    {

    }

    public override void SetupProjectile(Vector3 _pos, Vector3 _flyingDir, float _speed, int _damage, float _lifetime)
    {
        rb = GetComponent<Rigidbody>();
        if (!poolManager) poolManager = FindAnyObjectByType<PoolManager>();

        floorHit = false;
        transform.position = _pos;
        transform.rotation = Quaternion.LookRotation(_flyingDir);
        rb.linearVelocity = _flyingDir;
        damage = _damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Utils.CompareRootTag(other, playerTag))
        {
            PlayerHurt player = other.GetComponent<PlayerHurt>();

            if (player != null)
            {
                player.GetHit(damage);
            }
        }
        if (other.CompareTag(floorTag))
        {
            if (floorHit) 
                return; // 이미 바닥에 닿았으면 중복 처리 방지

            floorHit = true;

            StartCoroutine(ShootingCoroutine());
        }
    }

    IEnumerator ShootingCoroutine() 
    {
        GameObject effect = null;
        
        yield return poolManager.GetObject(explosionEffect, inst => effect = inst, effectParent);

        if (effect == null) yield break; 

        effect.transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        effect.transform.rotation = Quaternion.identity;

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps == null) yield break;

        StartCoroutine(TerminateCorutine(ps.main.duration, ps));
    }

    IEnumerator TerminateCorutine(float _duration, ParticleSystem _effect) 
    {
        _effect.Play();
        yield return new WaitForSeconds(_duration);
        _effect.Stop();
        poolManager.ReturnObject(_effect.gameObject);
        Terminate();
    }
}
