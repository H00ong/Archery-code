using Game.Enemies.Enum;
using System.Collections;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FlyingShoot : EnemyAttack, IAnimationListener
{
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject flyingProjectilePrefab;
    [SerializeField] Transform projectileParent;
    [SerializeField] Transform shootingPoint;
    [Header("Flying Shooting Tuning")]
    [SerializeField] float defaultFlyingProjectileSpeed = 10f;   // EnemyData
    [SerializeField] float defaultProjectileLifetime = 10f;
    [SerializeField] int defaultFlyingProjectileAtk = 5;        // EnemyData

    public override void Init(EnemyController c)
    {
        base.Init(c);

        if (!ctx.debugMode)
        {
            var shootingStats = stats.flyingShooting;
            defaultFlyingProjectileSpeed = shootingStats.flyingProjectileSpeed;
            defaultFlyingProjectileAtk = shootingStats.flyingProjectileAtk;
        }
    }


    public override void OnEnter()
    {
        base.OnEnter();
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
        StartCoroutine(FlyingShootingCoroutine());
    }

    IEnumerator FlyingShootingCoroutine()
    {
        GameObject go = null;

        yield return ctx.poolManager.GetObject(flyingProjectilePrefab, inst => go = inst, projectileParent);

        Vector3 flyingDir = Vector3.zero;

        float distance = Utils.GetXZDistance(shootingPoint.position, ctx.lastPlayerPosition);
        float flyTime = distance / defaultFlyingProjectileSpeed;
        float yVelocity = -Physics.gravity.y * flyTime / 2f - shootingPoint.position.y / flyTime; // Calculate the vertical velocity needed to reach the player

        flyingDir = Utils.GetDirectionVector(ctx.lastPlayerPosition, shootingPoint.position);
        flyingDir = flyingDir * defaultFlyingProjectileSpeed + Vector3.up * yVelocity;

        Projectile_Boss newProjectile = go.GetComponent<Projectile_Boss>();
        newProjectile.SetupProjectile(shootingPoint.position, flyingDir, -1f, defaultFlyingProjectileAtk, defaultProjectileLifetime);
    }
}
