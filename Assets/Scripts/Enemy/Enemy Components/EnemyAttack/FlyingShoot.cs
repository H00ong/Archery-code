using System.Collections;
using System.Collections.Generic;
using Enemies;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemy.Enemy_Components.EnemyAttack
{
    public class FlyingShoot : Enemies.EnemyAttack, IAnimationListener
    {
        [Header("Flying Shooting Tuning")]
        [SerializeField] private float defaultFlyingProjectileSpeed = 10f;
        [SerializeField] private float defaultProjectileLifetime = 10f;
        [SerializeField] private int defaultFlyingProjectileAtk = 5;
        
        private AssetReferenceGameObject _projectilePrefab;
        private List<Transform> _shootingPoints;

        private PoolManager _poolManager;

        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                var shootingStats = _stats.flyingShooting;
                defaultFlyingProjectileSpeed = shootingStats.flyingProjectileSpeed;
                defaultFlyingProjectileAtk = shootingStats.flyingProjectileAtk;
            }

            _poolManager = PoolManager.Instance;

            if (data is not FlyingShootData fData)
            {
                Debug.LogError("[FlyingShoot] Invalid module data provided!");
                return;
            }
            
            _projectilePrefab = fData.projectilePrefab;
            _shootingPoints = fData.GetShootingPoint(ctx);
        }

        public override void OnAnimEvent()
        {
            StartCoroutine(FlyingShootingCoroutine());
        }

        IEnumerator FlyingShootingCoroutine()
        {
            foreach (var point in  _shootingPoints)
            {
                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _poolManager.ProjectilePool)) 
                    yield return _poolManager.GetObject(_projectilePrefab, inst => go = inst, _poolManager.ProjectilePool);

                FlyingProjectile proj = go.GetComponent<FlyingProjectile>();
                
                var inst = new ShootingInstruction(
                    point.position,
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
}
