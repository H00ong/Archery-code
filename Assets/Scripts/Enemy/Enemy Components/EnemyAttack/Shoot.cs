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

            var shootingStats = _stat.shooting;
            _projectileSpeed = shootingStats.projectileSpeed;
            _projectileAtk = shootingStats.projectileAtk;

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
                _ctx.anim.SetInteger(AnimHashes.AttackIndex, _animIndex);
            }
        }


        public override void OnAnimEvent()
        {
            // Projectile.InitProjectile은 direction.y를 0으로 고정해 스폰 높이 그대로 수평 비행하므로,
            // dest.y 보정만으로는 부족하고 스폰 지점의 y도 같은 기준으로 맞춰야 한다.
            float fixedHeight = _ctx.lastPlayerPosition.y + destinationHeightOffset;

            var cachedPoints = new List<(Vector3 pos, Vector3 dest)>(_shootingPoints.Count);
            foreach (var point in _shootingPoints)
            {
                Vector3 spawnPos = point.position;
                spawnPos.y = fixedHeight;
                cachedPoints.Add((spawnPos, GetDestination(point)));
            }

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

        private const float destinationHeightOffset = .8f;
        private const float minTargetingDistance = 1f;

        private Vector3 GetDestination(Transform point)
        {
            if (_playerTargeting)
            {
                Vector3 playerPos = _ctx.lastPlayerPosition;
                Vector3 toPlayer = playerPos - point.position;
                toPlayer.y = 0f;

                // 발사 지점과 플레이어가 수평상 거의 겹치면 방향 벡터가 0에 가까워져
                // Projectile의 LookRotation이 엉뚱한(플레이어 뒤쪽 등) 방향을 향하게 되므로 최소 사거리를 보장한다.
                Vector3 dest = toPlayer.sqrMagnitude < minTargetingDistance * minTargetingDistance
                    ? point.position + point.forward * minTargetingDistance
                    : playerPos;

                dest.y = playerPos.y + destinationHeightOffset;
                return dest;
            }
            else
            {
                Vector3 forward = point.forward;
                Vector3 dest = point.position + forward * 100f;
                dest.y = _ctx.lastPlayerPosition.y + destinationHeightOffset;
                return dest;
            }
        }
    }
}

