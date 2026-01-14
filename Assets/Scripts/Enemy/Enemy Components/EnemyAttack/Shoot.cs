using System.Collections;
using System.Collections.Generic;
using Enemy;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemies.Enemy_Components.EnemyAttack
{
    public class Shoot : Enemies.EnemyAttack, IAnimationListener
    {
        [Header("Shooting Tuning")]
        [SerializeField] private float defaultProjectileSpeed = 10f;
        [SerializeField] private float defaultProjectileLifetime = 10f;
        [SerializeField] private int defaultProjectileAtk = 1;

        private AssetReferenceGameObject _projectilePrefab;
        private List<Transform> _shootingPoints;
        
        private PoolManager _poolManager;
        private Transform _projectileParent;
        
        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                var shootingStats = _stats.shooting;
                defaultProjectileSpeed = shootingStats.projectileSpeed;
                defaultProjectileAtk = shootingStats.projectileAtk;
            }

            _poolManager = PoolManager.Instance;
            _projectileParent = _poolManager.ProjectilePool;
            
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

            _ctx.SetAttackTrigger(false);
        }
        

        public override void OnAnimEvent()
        {
            StartCoroutine(ShootingCoroutine());
        }

        IEnumerator ShootingCoroutine()
        {
            foreach (var point in _shootingPoints) 
            {
                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _projectileParent))
                    yield return _poolManager.GetObject(_projectilePrefab, inst => go = inst, _projectileParent);
                
                Vector3 destDir = Vector3.zero;

                if (EnemyTagUtil.Has(_ctx.enemyTags, EnemyTag.PatternMove))
                    destDir = point.forward;
                else
                    destDir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, point.position);
                
                ShootingInstruction inst = new ShootingInstruction(
                    _ctx.transform.position,
                    _ctx.transform.position + destDir,
                    defaultProjectileSpeed,
                    defaultProjectileLifetime,
                    defaultProjectileAtk
                );
                
                Projectile proj = go.GetComponent<Projectile>();
                proj.InitProjectile(inst);

                go.SetActive(true);
            }
        }
    }
}
