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
        private const float _defaultProjectileLifetime = 10f;

        private float _projectileSpeed = 10f;
        private int _projectileAtk = 1;
        private bool _playerTargeting = false;

        private AssetReferenceGameObject _projectilePrefab;
        private List<Transform> _shootingPoints;
        private PoolManager _poolManager;
        EffectType _effectType;

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
            _playerTargeting = sData.playerTargeting;
            _effectType = sData.GetEffectType();
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
            StartCoroutine(ShootingCoroutine());
        }

        IEnumerator ShootingCoroutine()
        {
            foreach (var point in _shootingPoints)
            {
                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _poolManager.ProjectilePool))
                    yield return _poolManager.GetObject(_projectilePrefab, inst => go = inst, _poolManager.ProjectilePool);

                Vector3 dest = GetDestination(point);

                var damageInfo = new DamageInfo(_projectileAtk, _effectType, _ctx.gameObject);

                ShootingInstruction inst = new ShootingInstruction(
                    point.position,
                    dest,
                    _projectileSpeed,
                    _defaultProjectileLifetime,
                    damageInfo
                );

                Projectile proj = go.GetComponent<Projectile>();
                proj.InitProjectile(inst);

                go.SetActive(true);
            }
        }

        private Vector3 GetDestination(Transform point)
        {
            if (_playerTargeting)
            {
                return _ctx.lastPlayerPosition;
            }
            else
            {
                Vector3 forward = point.forward;
                Vector3 dest = point.position + forward * 100f;
                return dest;
            }
        }
    }
}

