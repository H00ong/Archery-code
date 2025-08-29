using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile_Enemy : Projectile
{
    protected string playerTag = "Player";
    protected string obstacleTag = "Obstacle";
    protected string enemyProjectileTag = "EnemyProjectile";
    protected string floorTag = "Floor";

    protected override void Start()
    {
        base.Start();

        gameObject.tag = enemyProjectileTag;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void SetupProjectile(Vector3 _pos, Vector3 _dir, float _speed, int _damage, float _lifetime)
    {
        base.SetupProjectile(_pos, _dir, _speed, _damage, _lifetime);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (Utils.CompareRootTag(other, playerTag))
        {
            PlayerHurt player = other.GetComponent<PlayerHurt>();

            if (player != null)
            {
                player.GetHit(damage);
                Terminate();
            }
        }
        else if (other.CompareTag(obstacleTag))
        {
            // Handle collision with obstacles if needed
            Terminate();
        }
        else if (other.CompareTag(floorTag))
        {
            Terminate();
        }
    }
}
