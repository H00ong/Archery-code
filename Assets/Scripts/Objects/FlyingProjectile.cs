

using UnityEngine;

namespace Objects
{
    public class FlyingProjectile : Projectile
    {
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