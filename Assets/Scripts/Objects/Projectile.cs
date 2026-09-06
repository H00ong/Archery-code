using System;
using System.Collections.Generic;
using Enemy;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Objects
{
    public readonly struct ShootingInstruction
    {
        public readonly Vector3 Position;
        public readonly Vector3 Destination;
        public readonly float Speed;
        public readonly float Lifetime;
        public readonly DamageInfo DamageInfo;

        public ShootingInstruction(Vector3 pos, Vector3 destination, float speed, float lifetime, DamageInfo damageInfo)
        {
            Position = pos;
            Destination = destination;
            Speed = speed;
            Lifetime = lifetime;
            DamageInfo = damageInfo;
        }
    }

    /// <summary> 스킬로 부여되는 투사체 추가 거동. InitProjectile 직후에 적용한다. </summary>
    public readonly struct ProjectileModifier
    {
        public readonly int PierceCount;      // 관통 가능한 적 수
        public readonly int ReflectCount;     // 장애물 반사 횟수
        public readonly float HomingTurnSpeed; // 유도 선회 속도 (deg/s). 0이면 유도 없음

        public ProjectileModifier(int pierceCount, int reflectCount, float homingTurnSpeed)
        {
            PierceCount = pierceCount;
            ReflectCount = reflectCount;
            HomingTurnSpeed = homingTurnSpeed;
        }
    }

    public class Projectile : SceneObject
    {
        [Header("Identity")]
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private bool destroyOnFloor = true;
        [SerializeField] private bool destroyOnObstacle = true;

        [Header("Components")]
        [SerializeField] protected Rigidbody rigidBody;
        [SerializeField] protected Collider projectileCollider;
        
        [Header("Required Objects")]
        [SerializeField] AssetReferenceGameObject explosionEffect;

        [Header("Reflect")]
        [SerializeField] private float reflectSkinWidth = 0.05f;

        private float _lifetimeTimer;
        protected float _lifetime;
        protected bool _isActive;
        protected bool _effectSpawned;

        private float _speed;
        private int _pierceRemaining;
        private int _reflectRemaining;
        private float _homingTurnSpeed;
        private Transform _homingTarget;
        private int _lastReflectFrame = -1;

        private bool HasExplosion => explosionEffect != null && explosionEffect.RuntimeKeyIsValid();

        private readonly List<GameObject> hitObjects = new List<GameObject>();

        protected DamageInfo DamageInfo { get; set; }

        protected virtual void Start()
        {
            projectileCollider = GetComponentInChildren<Collider>();
            projectileCollider.isTrigger = true;
        }

        protected virtual void Update()
        {
            CheckLifeTime();
            UpdateHoming();
        }

        private void CheckLifeTime()
        {
            if (!_isActive) return;
            
            _lifetimeTimer += Time.deltaTime;

            if (_lifetimeTimer >= _lifetime)
            {
                Terminate();
            }
        }

        public virtual void InitProjectile(ShootingInstruction instruction)
        {
            _isActive = true;
            hitObjects.Clear(); // 풀에서 재사용될 때 이전 히트 기록 제거

            DamageInfo = instruction.DamageInfo;
            _lifetime = instruction.Lifetime;
            _speed = instruction.Speed;

            // 풀 재사용 시 이전 탄의 스킬 옵션이 남지 않도록 초기화한다.
            _pierceRemaining = 0;
            _reflectRemaining = 0;
            _homingTurnSpeed = 0f;
            _homingTarget = null;

            transform.position = instruction.Position;
            var direction = instruction.Destination - instruction.Position;
            direction.y = 0f;
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
            rigidBody.linearVelocity = direction * instruction.Speed;
        }

        /// <summary> 관통·반사·유도 옵션을 적용한다. InitProjectile 다음에 호출해야 한다. </summary>
        public void ApplyModifier(in ProjectileModifier modifier)
        {
            _pierceRemaining = modifier.PierceCount;
            _reflectRemaining = modifier.ReflectCount;
            _homingTurnSpeed = modifier.HomingTurnSpeed;
        }

        private void UpdateHoming()
        {
            if (!_isActive || _homingTurnSpeed <= 0f) return;

            if (!IsValidHomingTarget(_homingTarget))
                _homingTarget = FindNearestEnemy();

            if (_homingTarget == null)
            {
                // 타겟이 없으면 방향을 틀지 않고 현재 진행 방향을 그대로 유지한 채 직진한다.
                Vector3 keepDir = rigidBody.linearVelocity;
                keepDir.y = 0f;
                if (keepDir.sqrMagnitude > 0.0001f)
                    rigidBody.linearVelocity = keepDir.normalized * _speed;
                return;
            }

            Vector3 desired = _homingTarget.position - transform.position;
            desired.y = 0f;
            if (desired.sqrMagnitude < 0.0001f) return;

            Vector3 current = rigidBody.linearVelocity;
            current.y = 0f;
            if (current.sqrMagnitude < 0.0001f) return;

            float maxRadians = _homingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(current.normalized, desired.normalized, maxRadians, 0f);

            rigidBody.linearVelocity = newDir * _speed;
            transform.rotation = Quaternion.LookRotation(newDir);
        }

        /// <summary> 다시 잡을 필요 없는 타겟인지(파괴됨/비활성화/사망) 판단한다. </summary>
        private static bool IsValidHomingTarget(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;

            var enemy = target.GetComponent<EnemyController>();
            return enemy == null || enemy.CurrentState != EnemyState.Dead;
        }

        private Transform FindNearestEnemy()
        {
            var manager = EnemyManager.Instance;
            if (manager == null) return null;

            Transform nearest = null;
            float minSqr = float.MaxValue;

            foreach (var enemy in manager.Enemies)
            {
                if (enemy == null || enemy.CurrentState == EnemyState.Dead) continue;

                float sqr = (enemy.transform.position - transform.position).sqrMagnitude;
                if (sqr >= minSqr) continue;

                minSqr = sqr;
                nearest = enemy.transform;
            }

            return nearest;
        }

        protected void Terminate()
        {
            _isActive = false;

            _lifetimeTimer = 0f;

            if (HasExplosion)
            {
                ExplosionAsync().Forget();
                return;
            }

            PoolManager.Instance.ReturnObject(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!_isActive) return;

            var damage = other.GetComponentInParent<IDamageable>();

            // 발사자 자신은 타격 대상에서 제외 (레이어/태그로는 걸러지지 않음)
            bool isSelf = damage != null && DamageInfo.attackSource != null
                          && other.transform.IsChildOf(DamageInfo.attackSource.transform);

            if (damage != null && !isSelf)
            {
                if (CheckHitObjects(other.gameObject))
                {
                    damage.TakeDamage(DamageInfo);

                    if (_pierceRemaining > 0)
                    {
                        _pierceRemaining--;
                        return;
                    }

                    if (destroyOnHit) 
                    {
                        Terminate();
                        return;
                    }
                }
            }

            if (destroyOnFloor && other.CompareTag(Utils.ToString(TagType.Floor)))
            {
                Terminate();
                return;
            }
            
            if (destroyOnObstacle && other.CompareTag(Utils.ToString(TagType.Obstacle)))
            {
                // 반사가 실패했는데 살려두면 속도가 그대로 유지돼 벽을 관통해버린다.
                if (_reflectRemaining <= 0 || !TryReflect(other))
                {
                    Terminate();
                    return;
                }

                _reflectRemaining--;
                return;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_isActive || !destroyOnObstacle) return;
            if (Time.frameCount == _lastReflectFrame) return;
            if (!other.CompareTag(Utils.ToString(TagType.Obstacle))) return;

            // 밀어낸 뒤에도 벽 안에 남아 있으면 반대편으로 빠져나가기 전에 소멸시킨다.
            Terminate();
        }

        /// <summary> 반사에 성공하면 true. false면 호출측이 반드시 소멸시켜야 관통을 막을 수 있다. </summary>
        private bool TryReflect(Collider surface)
        {
            Vector3 velocity = rigidBody.linearVelocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude < 0.0001f) return false;

            if (!TryResolveSurface(surface, velocity, out Vector3 outwardNormal, out float pushDistance))
                return false;

            Vector3 reflected = Vector3.Reflect(velocity.normalized, outwardNormal);
            reflected.y = 0f;
            if (reflected.sqrMagnitude < 0.0001f) return false;

            reflected.Normalize();

            // 트리거 감지 시점엔 이미 벽 안쪽이라, 밖으로 밀어내지 않으면 다음 충돌이 감지되지 않는다.
            transform.position += outwardNormal * (pushDistance + reflectSkinWidth);

            rigidBody.linearVelocity = reflected * _speed;
            transform.rotation = Quaternion.LookRotation(reflected);

            hitObjects.Clear(); // 반사 후엔 이미 맞힌 적도 다시 타격 가능
            _lastReflectFrame = Time.frameCount;
            return true;
        }

        /// <summary> 진행 방향과 마주보는 XZ 법선과, 벽 밖으로 빠져나가는 데 필요한 거리를 구한다. </summary>
        private bool TryResolveSurface(Collider surface, Vector3 velocity, out Vector3 outwardNormal, out float pushDistance)
        {
            outwardNormal = Vector3.zero;
            pushDistance = 0f;

            if (projectileCollider != null && Physics.ComputePenetration(
                    projectileCollider, projectileCollider.transform.position, projectileCollider.transform.rotation,
                    surface, surface.transform.position, surface.transform.rotation,
                    out Vector3 penetrationNormal, out float penetrationDistance))
            {
                // y를 지우면 0이 될 수 있으므로(윗면/아랫면 관통), 그때는 아래 대체 계산으로 넘어간다.
                penetrationNormal.y = 0f;
                if (penetrationNormal.sqrMagnitude >= 0.0001f && Vector3.Dot(velocity, penetrationNormal) < 0f)
                {
                    outwardNormal = penetrationNormal.normalized;
                    pushDistance = Mathf.Max(0f, penetrationDistance);
                    return true;
                }
            }

            Vector3 closest = SupportsClosestPoint(surface) ? surface.ClosestPoint(transform.position) : transform.position;
            Vector3 normal = transform.position - closest;
            normal.y = 0f;
            if (normal.sqrMagnitude >= 0.0001f && Vector3.Dot(velocity, normal) < 0f)
            {
                outwardNormal = normal.normalized;
                pushDistance = normal.magnitude;
                return true;
            }

            return TryResolveBoundsExit(surface.bounds, velocity, out outwardNormal, out pushDistance);
        }

        /// <summary> 투사체가 파고든 면 중 진행 방향과 마주보면서 가장 가까운 쪽을 탈출 방향으로 고른다. </summary>
        private bool TryResolveBoundsExit(Bounds bounds, Vector3 velocity, out Vector3 outwardNormal, out float pushDistance)
        {
            outwardNormal = Vector3.zero;
            pushDistance = 0f;

            Vector3 position = transform.position;
            float best = float.MaxValue;

            TakeExitFaceIfCloser(Vector3.left, position.x - bounds.min.x, velocity, ref best, ref outwardNormal);
            TakeExitFaceIfCloser(Vector3.right, bounds.max.x - position.x, velocity, ref best, ref outwardNormal);
            TakeExitFaceIfCloser(Vector3.back, position.z - bounds.min.z, velocity, ref best, ref outwardNormal);
            TakeExitFaceIfCloser(Vector3.forward, bounds.max.z - position.z, velocity, ref best, ref outwardNormal);

            if (outwardNormal == Vector3.zero) return false;

            pushDistance = Mathf.Max(0f, best);
            return true;
        }

        private static void TakeExitFaceIfCloser(Vector3 normal, float distance, Vector3 velocity, ref float best, ref Vector3 outwardNormal)
        {
            if (Vector3.Dot(velocity, normal) >= 0f || distance >= best) return;

            best = distance;
            outwardNormal = normal;
        }

        /// <summary> Collider.ClosestPoint는 Box/Sphere/Capsule/볼록 MeshCollider에서만 지원된다. </summary>
        private static bool SupportsClosestPoint(Collider collider)
        {
            return collider switch
            {
                BoxCollider or SphereCollider or CapsuleCollider => true,
                MeshCollider meshCollider => meshCollider.convex,
                _ => false
            };
        }

        async Awaitable ExplosionAsync()
        {
            if (!PoolManager.Instance.TryGetObject(explosionEffect, out var go, PoolManager.Instance.effectPool))
                go = await PoolManager.Instance.GetObjectAsync(explosionEffect, PoolManager.Instance.effectPool);

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (!go)
            {
                Debug.LogError("[Projectile] ExplosionAsync: failed to get explosion effect from pool.");
                return;
            }

            ParticleEffect effect = go.GetOrAddComponent<ParticleEffect>();
            effect.InitializeEffect(transform.position);

            PoolManager.Instance.ReturnObject(gameObject);
        }

        private bool CheckHitObjects(GameObject hitObject)
        {
            if (hitObjects.Contains(hitObject)) 
                return false;
            
            hitObjects.Add(hitObject);
            return true;
        }
    }
}