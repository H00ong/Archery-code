using System.Collections.Generic;
using Enemy;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemy
{
    public class FlyingShoot : Enemy.EnemyAttack, IAnimationListener
    {
        private readonly float _projectileLifeTime = 10f;
        
        private AssetReferenceGameObject _projectilePrefab;
        private float _flyingProjectileSpeed = 10f; 
        private int _flyingProjectileAtk = 5;
        private PoolManager _poolManager;
        
        private List<Transform> _shootingPoints;

        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                var shootingStats = _stat.flyingShooting;
                _flyingProjectileSpeed = shootingStats.flyingProjectileSpeed;
                _flyingProjectileAtk = shootingStats.flyingProjectileAtk;
            }

            _poolManager = PoolManager.Instance;

            if (data is not FlyingShootData fData)
            {
                Debug.LogError("[FlyingShoot] Invalid module data provided!");
                return;
            }
            
            _projectilePrefab = fData.projectilePrefab;
            _shootingPoints = fData.GetShootingPoint(ctx);
            _animIndex = fData.attackIndex;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (_ctx.HasMultiAttackModules)
            {
                _ctx.anim.SetFloat(AnimHashes.AttackIndex, _animIndex);
            }
        }

        public override void OnAnimEvent()
        {
            FlyingShootAsync().Forget();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        async Awaitable FlyingShootAsync()
        {
            foreach (var point in  _shootingPoints)
            {
                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _poolManager.projectilePool)) 
                    go = await _poolManager.GetObjectAsync(_projectilePrefab, _poolManager.projectilePool);

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (!go)
                {
                    Debug.LogError("[FlyingShoot] FlyingShootAsync: failed to get projectile from pool.");
                    return;
                }

                FlyingProjectile proj = go.GetComponent<FlyingProjectile>();
                
                var damageInfo = new DamageInfo(_flyingProjectileAtk, EffectType.Normal, _ctx.gameObject);
                
                var inst = new ShootingInstruction(
                    point.position,
                    _player.transform.position,
                    _flyingProjectileSpeed,
                    _projectileLifeTime,
                    damageInfo
                );
                
                proj.InitProjectile(inst);
                go.SetActive(true);
            }
        }
    }
}
