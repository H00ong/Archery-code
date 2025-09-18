using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Projectile_Player : Projectile
{
    readonly string obstacleTag = "Obstacle";
    readonly string floorTag = "Floor";
    readonly string playerProjectileTag = "PlayerProjectile";

    protected override void Start()
    {
        base.Start();

        gameObject.tag = playerProjectileTag;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void SetupProjectile(Vector3 _pos, Vector3 _dir, float _speed, int _damage, float _lifetime, bool _isFlying)
    {
        base.SetupProjectile(_pos, _dir, _speed, _damage, _lifetime, _isFlying);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnemyController>() != null)
        {
            EnemyController enemy = other.GetComponentInParent<EnemyController>();

            if (enemy != null)
            {
                enemy.GetHit(damage);
                Terminate();
            }
        }
        else if (other.CompareTag(obstacleTag))
        {
            Terminate();
        }
        else if (other.CompareTag(floorTag)) 
        {
            Terminate();
        }
    }
}
