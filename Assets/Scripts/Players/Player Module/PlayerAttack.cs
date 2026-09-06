using Game.Player;
using Game.Player.Attack;
using System.Collections.Generic;
using Enemy;
using Managers;
using Objects;
using Players;
using Players.SkillModule;
using Stat;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Player.Attack
{
    public struct ShotInstruction
    {
        public Vector3 shootingPos;
        public Vector3 shootingDir;
    }
}

public class PlayerAttack : MonoBehaviour
{
    [Header("Debug Mode")]
    [SerializeField] bool debugMode;
    [Space]
    [Header("Required Objects")]
    [SerializeField] private Transform shootingPos;
    [Space]
    [Header("Player Attack Info")]
    [SerializeField] private float defaltProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [SerializeField] private float multishotTimeInterval = .3f;

    private AssetReferenceGameObject _playerProjectile;
    private PlayerSkill _playerSkill;
    private PlayerStat _stat;
    private EnemyController _currentTarget;

    private readonly List<ShotInstruction> shotPlan = new(10);


    // TODO : json 파일에서 읽어오는 방식으로 바뀌어야 함
    public void Init() 
    {
        _stat = PlayerController.Instance.Stat;
        _playerSkill = PlayerController.Instance.Skill;
        _playerProjectile = CharacterManager.Instance.CurrentProjectilePrefab;
    }

    public void Attack()
    {
        EnemyController target = GetEnemyTarget();

        if (target)
        {
            _currentTarget = target;
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
    }

    private EnemyController GetEnemyTarget()
    {
        EnemyController target = null;
        float minDistance = float.MaxValue;

        var list = EnemyManager.Instance.Enemies;

        foreach (var enemy in list)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                target = enemy;
            }
        }

        return target;
    }

    #region Animation Events
    public void Shoot()
    {
        ShootAsync().Forget();
    }

    async Awaitable ShootAsync()
    {
        var plan = GetShotPlan();
        var pool = PoolManager.Instance;
        var skills = _playerSkill.acquiredSkillModule;

        if (plan == null || plan.Count == 0) return;
        
        int countOfMultiShot = 1;
        if (skills.TryGetValue("MultiShot", out var mod)
            && mod is MultiShot multi)
        {
            countOfMultiShot = Mathf.Max(1, multi.MultiShotCount);
        }

        var projectileModifier = BuildProjectileModifier();

        for (int i = 0; i < countOfMultiShot; i++)
        {
            var bulletWave = new List<GameObject>();

            for (int j = 0; j < plan.Count; j++)
            {
                var inst = plan[j];

                if (!pool.TryGetObject(_playerProjectile, out var go, pool.projectilePool))
                {
                    go = await pool.GetObjectAsync(_playerProjectile, pool.projectilePool);
                }

                destroyCancellationToken.ThrowIfCancellationRequested();

                if (!go) return;

                DamageInfo damageInfo = new DamageInfo(_stat.AttackPower, _stat.AttackEffectType, _stat, transform.gameObject);

                Objects.ShootingInstruction projInst = new Objects.ShootingInstruction(
                    inst.shootingPos,
                    inst.shootingPos + inst.shootingDir,
                    _stat.ProjectileSpeed,
                    defaltProjectileLifetime,
                    damageInfo
                );

                var proj = go.GetComponent<Projectile>();
                proj.InitProjectile(projInst);
                proj.ApplyModifier(projectileModifier);

                bulletWave.Add(go);
            }

            // Activate all bullets in the wave
            foreach (var bullet in bulletWave)
            {
                bullet.SetActive(true);
            }
            
            if (i < countOfMultiShot - 1 && multishotTimeInterval > 0f)
                await Awaitable.WaitForSecondsAsync(multishotTimeInterval, destroyCancellationToken);
        }
    }

    private IReadOnlyList<ShotInstruction> GetShotPlan()
    {
        RebuildShotPlan();

        return shotPlan;
    }

    private ProjectileModifier BuildProjectileModifier()
    {
        var skills = _playerSkill.acquiredSkillModule;

        int pierce = skills.TryGetValue("PierceShot", out var p) && p is PierceShot pierceShot
            ? pierceShot.PierceCount : 0;

        int reflect = skills.TryGetValue("ReflectShot", out var r) && r is ReflectShot reflectShot
            ? reflectShot.ReflectCount : 0;

        float homing = skills.TryGetValue("HomingShot", out var h) && h is HomingShot homingShot
            ? homingShot.TurnSpeed : 0f;

        return new ProjectileModifier(pierce, reflect, homing);
    }

    private void RebuildShotPlan()
    {
        shotPlan.Clear();
        
        Vector3 dir = (_currentTarget.transform.position - transform.position).normalized;
        Vector3 pos = shootingPos.position;

        ShotInstruction inst = new ShotInstruction { shootingDir = dir, shootingPos = pos };

        if (_playerSkill.acquiredSkillModule.TryGetValue("HorizontalShot", out var horizon))
        {
            if (horizon is IShootContributor shotMod)
            {
                shotMod.AddBullet(shotPlan, inst);
            }
        }
        else 
        {
            shotPlan.Add(inst);
        }

        if (_playerSkill.acquiredSkillModule.TryGetValue("DiagonalShot", out var diagonal)) 
        {
            if (diagonal is IShootContributor shotMod) 
            {
                shotMod.AddBullet(shotPlan, inst);
            }
        }
    }
    #endregion

    #region Skill Methods
    public void UpdateAttackSpeed(float modifier)
    {
        _stat.ApplyAttackSpeedModifier(modifier);
        PlayerController.Instance.Anim.SetFloat(AnimHashes.AttackSpeed, _stat.AttackSpeed);
    }

    #endregion
}
