using Game.Enemies;
using Game.Enemies.Enum;
using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Shoot : EnemyAttack, IAnimationListener
{
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject projectilePrefab;
    [SerializeField] Transform projectileParent;
    [SerializeField] Transform shootingPoint;
    [Header("Shooting Tuning")]
    [SerializeField] float defaultProjectileSpeed = 10f; // EnemyData
    [SerializeField] float defaultProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [SerializeField] int defaultProjectileAtk = 1; // EnemyData
    bool isPatternMove 
    {
        get => EnemyTagUtil.Has(ctx.enemyTags, EnemyTag.PatternMove);
    }
    
    public override void Init(EnemyController c)
    {
        base.Init(c);

        if (!ctx.debugMode)
        {
            var shootingStats = stats.shooting;
            defaultProjectileSpeed = shootingStats.projectileSpeed;
            defaultProjectileAtk   = shootingStats.projectileAtk;
        }
    }

    public override void OnEnter()
    {
        // 방향 전환 없어야 함. pattern move -> shoot 할때는 transform.rotation 조정이 없어야 함.
        player = ctx.player;

        ctx.SetAttackTrigger(false);
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Tick()
    {
        base.Tick(); // empty
    }

    public override void OnAnimEvent()
    {
        StartCoroutine(ShootingCoroutine());
    }

    IEnumerator ShootingCoroutine()
    {
        GameObject go = null;

        yield return ctx.poolManager.GetObject(projectilePrefab, inst => go = inst, projectileParent);

        Vector3 flyingDir = Vector3.zero;

        if (isPatternMove)
            flyingDir = Utils.GetDirectionVector(ctx.lastPlayerPosition, shootingPoint.position);
        else
            flyingDir = shootingPoint.forward;

        Projectile_Enemy newProjectile = go.GetComponent<Projectile_Enemy>();
        newProjectile.SetupProjectile(shootingPoint.position, flyingDir, defaultProjectileSpeed, defaultProjectileAtk, defaultProjectileLifetime);
    }
}
