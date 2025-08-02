using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Projectile_Player : Projectile
{
    string enemyTag = "Enemy";
    string obstacleTag = "Obstacle";
    string playerProjectileTag = "PlayerProjectile";

    protected override void Start()
    {
        base.Start();

        gameObject.tag = playerProjectileTag;
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
        if (Utils.CompareTag(other, enemyTag))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.GetHit(damage);
                Terminate();
            }
        }
        else if (Utils.CompareTag(other, obstacleTag))
        {
            Terminate();
        }
    }
}
