

using UnityEngine;

namespace Objects
{
    public class FlyingProjectile : Projectile
    {
        [Header("Overlap Gizmo")]
        [SerializeField] private bool isBoss;
        // Ranged Default Variant(scale 0.75)의 충돌 반경 0.67 x 0.75
        [SerializeField] private float radius = 0.5f;
        // Boss Default 프리팹의 충돌 반경 0.67 x scale 1.04
        [SerializeField] private float bossRadius = 0.7f;

        private void OnDrawGizmos()
        {
            Gizmos.color = isBoss ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
        }

        private float ExplosionRadius => isBoss ? bossRadius : radius;

        // 직격으로 이미 맞은 플레이어는 CheckHitObjects가 걸러내므로 폭발 피해가 중복되지 않는다.
        protected override void OnTerminate()
        {
            var playerTag = Utils.ToString(TagType.Player);
            var colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, ~0, QueryTriggerInteraction.Ignore);

            foreach (var col in colliders)
            {
                GameObject root = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
                if (!root.CompareTag(playerTag)) continue;

                var damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || !CheckHitObjects(damageable)) continue;

                damageable.TakeDamage(DamageInfo);
            }
        }

        public override void InitProjectile(ShootingInstruction instruction)
        {
            // 부모 초기화를 거쳐야 DamageInfo/hitObjects/피어스·반사·유도 상태가 정상적으로 세팅된다 (누락 시 OnTriggerEnter에서 DamageInfo null 참조).
            base.InitProjectile(instruction);

            var pos = instruction.Position;
            var dest = instruction.Destination;

            var distance = Utils.GetXZDistance(dest, pos);
            var flyTime = distance / instruction.Speed;
            var yVelocity = -Physics.gravity.y * flyTime / 2f - pos.y / flyTime;
            
            _lifetime = ((flyTime > instruction.Lifetime) ? flyTime : instruction.Lifetime) + 1f;
            Vector3 flyingDir = Utils.GetXZDirectionVector(instruction.Destination, instruction.Position);
            flyingDir = flyingDir * instruction.Speed + Vector3.up * yVelocity;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(flyingDir);
            rigidBody.linearVelocity = flyingDir;
        }
    }
}