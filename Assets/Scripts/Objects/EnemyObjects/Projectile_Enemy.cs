using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(Rigidbody))]
public class Projectile_Enemy : Projectile
{
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject explosionEffect;
    [SerializeField] Transform effectParent;

    bool playerHit = false;
    bool hit = false;

    private readonly string playerTag = "Player";
    private readonly string obstacleTag = "Obstacle";
    private readonly string enemyProjectileTag = "EnemyProjectile";
    private readonly string floorTag = "Floor";

    protected override void Start()
    {
        base.Start();

        gameObject.tag = enemyProjectileTag;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void SetupProjectile(Vector3 _pos, Vector3 _dir, float _speed, int _damage, float _lifetime, bool _isFlying)
    {
        base.SetupProjectile(_pos, _dir, _speed, _damage, _lifetime, _isFlying);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PlayerHurt player = other.GetComponent<PlayerHurt>();

            if (player != null && !playerHit)
            {
                playerHit = true;
                player.GetHit(damage);
            }
        }
        if (other.CompareTag(obstacleTag) || other.CompareTag(floorTag))
            OnHit();
    }

    private void OnHit()
    {
        if (hit) return; // 이미 바닥에 닿았으면 중복 처리 방지

        hit = true;

        if (hasExplosionEffect)
            StartCoroutine(ExplosionCoroutine());
        else
            Terminate();
    }

    IEnumerator ExplosionCoroutine()
    {
        GameObject go = null;
        effectParent = poolManager.EffectPool;
        
        yield return poolManager.GetObject(explosionEffect, inst => go = inst, effectParent);

        if (go == null) yield break;

        ParticleEffect effect = go.GetOrAddComponent<ParticleEffect>();
        effect.SetupEffect(transform.position);
        Terminate();
    }
}