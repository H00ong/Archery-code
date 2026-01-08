using System.Collections;
using Enemy;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemies.Enemy_Components.EnemyAttack
{
    public class FlyingShoot : Enemies.EnemyAttack, IAnimationListener
    {
        [Header("Required Objects")]
        [SerializeField] private AssetReferenceGameObject flyingProjectilePrefab;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private Transform shootingPoint;
        [Header("Flying Shooting Tuning")]
        [SerializeField] private float defaultFlyingProjectileSpeed = 10f;
        [SerializeField] private float defaultProjectileLifetime = 10f;
        [SerializeField] private int defaultFlyingProjectileAtk = 5;

        private PoolManager _poolManager;

        public override void Init(EnemyController c)
        {
            base.Init(c);

            if (!_ctx.isDebugMode)
            {
                var shootingStats = _stats.flyingShooting;
                defaultFlyingProjectileSpeed = shootingStats.flyingProjectileSpeed;
                defaultFlyingProjectileAtk = shootingStats.flyingProjectileAtk;
            }

            _poolManager = PoolManager.Instance;
            projectileParent = _poolManager.ProjectilePool;
        }

        public override void OnAnimEvent()
        {
            StartCoroutine(FlyingShootingCoroutine());
        }

        IEnumerator FlyingShootingCoroutine()
        {
            if (!_poolManager.TryGetObject(flyingProjectilePrefab, out var go, projectileParent)) 
                yield return _poolManager.GetObject(flyingProjectilePrefab, inst => go = inst, projectileParent);

            FlyingProjectile proj = go.GetComponent<FlyingProjectile>();
            var inst = new ShootingInstruction(
                shootingPoint.transform.position,
                _player.transform.position,
                defaultFlyingProjectileSpeed,
                defaultProjectileLifetime,
                defaultFlyingProjectileAtk
            );
            
            proj.InitProjectile(inst);
            go.SetActive(true);
        }
    }
}
