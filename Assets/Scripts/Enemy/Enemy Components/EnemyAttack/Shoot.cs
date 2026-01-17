using System.Collections;
using System.Collections.Generic;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemy
{
    public class Shoot : EnemyAttack, IAnimationListener
    {
        private readonly float _defaultProjectileLifetime = 10f;
        
        private float _projectileSpeed = 10f;
        private int _projectileAtk = 1;

        private AssetReferenceGameObject _projectilePrefab;
        private List<Transform> _shootingPoints;
        
        private PoolManager _poolManager;
        
        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                var shootingStats = _stats.shooting;
                _projectileSpeed = shootingStats.projectileSpeed;
                _projectileAtk = shootingStats.projectileAtk;
            }

            _poolManager = PoolManager.Instance;
            
            if (data is not ShootData sData)
            {
                Debug.LogError("[Shoot] Invalid module data provided!");
                return;
            }
            
            _projectilePrefab = sData.projectilePrefab;
            _shootingPoints = sData.GetShootingPoint(ctx);
        }

        public override void OnEnter()
        {
            _player = _ctx.player;

            if (_ctx.HasMultiAttackModules)
            {
                _ctx.anim.SetFloat(AnimHashes.AttackIndex, _animIndex);
            }
            
            _ctx.SetAttackEndTrigger(false);
        }
        

        public override void OnAnimEvent()
        {
            StartCoroutine(ShootingCoroutine());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        IEnumerator ShootingCoroutine()
        {
            foreach (var point in _shootingPoints) 
            {
                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _poolManager.ProjectilePool))
                    yield return _poolManager.GetObject(_projectilePrefab, inst => go = inst, _poolManager.ProjectilePool);
                
                Vector3 destDir = Vector3.zero;

                if (EnemyTagUtil.Has(_ctx.enemyTags, EnemyTag.PatternMove))
                    destDir = point.forward;
                else
                    destDir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, point.position);
                
                ShootingInstruction inst = new ShootingInstruction(
                    _ctx.transform.position,
                    _ctx.transform.position + destDir,
                    _projectileSpeed,
                    _defaultProjectileLifetime,
                    _projectileAtk
                );
                
                Projectile proj = go.GetComponent<Projectile>();
                proj.InitProjectile(inst);

                go.SetActive(true);
            }
        }
    }
}
