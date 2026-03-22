using System.Collections.Generic;
using Managers;
using Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Enemy
{
    public class Shoot : EnemyAttack, IAnimationListener
    {
        private const float defaultProjectileLifetime = 10f;

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
                var shootingStats = _stat.shooting;
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
            var cachedPoints = new List<(Vector3 pos, Vector3 dest)>(_shootingPoints.Count);
            foreach (var point in _shootingPoints)
                cachedPoints.Add((point.position, GetDestination(point)));

            ShootAsync(cachedPoints).Forget();
        }

        async Awaitable ShootAsync(List<(Vector3 pos, Vector3 dest)> cachedPoints)
        {
            foreach (var (spawnPos, dest) in cachedPoints)
            {

                if (!_poolManager.TryGetObject(_projectilePrefab, out var go, _poolManager.projectilePool))
                    go = await _poolManager.GetObjectAsync(_projectilePrefab, _poolManager.projectilePool);

                destroyCancellationToken.ThrowIfCancellationRequested();

                var damageInfo = new DamageInfo(_projectileAtk, _effectType, _ctx.gameObject);

                ShootingInstruction inst = new ShootingInstruction(
                    spawnPos,
                    dest,
                    _projectileSpeed,
                    defaultProjectileLifetime,
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

