using Game.Enemies;
using Game.Enemies.Enum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Shoot : EnemyAttack, IAnimationListener
{
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject projectilePrefab;
    [SerializeField] Transform projectileParent;
    [SerializeField] List<Transform> shootingPoint;
    [Header("Shooting Tuning")]
    [SerializeField] float defaultProjectileSpeed = 10f; // EnemyData
    [SerializeField] float defaultProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [SerializeField] int defaultProjectileAtk = 1; // EnemyData
    bool IsPatternMove
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
            defaultProjectileAtk = shootingStats.projectileAtk;
        }

        projectileParent = ctx.poolManager.ProjectilePool;
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

        int count = shootingPoint.Count;

        foreach (var point in shootingPoint) 
        {
            yield return ctx.poolManager.GetObject(projectilePrefab, inst => go = inst, projectileParent);

            Vector3 flyingDir = Vector3.zero;

            if (IsPatternMove)
                flyingDir = point.forward;
            else
                flyingDir = Utils.GetDirectionVector(ctx.lastPlayerPosition, point.position);

            Projectile_Enemy newProjectile = go.GetComponent<Projectile_Enemy>();
            newProjectile.SetupProjectile(point.position,
                                          flyingDir,
                                          defaultProjectileSpeed,
                                          defaultProjectileAtk,
                                          defaultProjectileLifetime,
                                          _isFlying: false);

            go.SetActive(true);
        }
    }
}
