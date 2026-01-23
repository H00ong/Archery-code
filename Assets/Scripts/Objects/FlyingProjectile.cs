

using UnityEngine;

namespace Objects
{
    public class FlyingProjectile : Projectile
    {
        public override void InitProjectile(ShootingInstruction instruction)
        {
            _isActive = true;
            
            var pos = instruction.Position;
            var dest = instruction.Destination;
            var time = instruction.Lifetime;
            var speed = instruction.Speed;
            var dmg = instruction.DamageInfo;

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