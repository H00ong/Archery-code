using Game.Player;
using Game.Player.Attack;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Entities.UniversalDelegates;
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
    [Header("Required Components")]
    [SerializeField] PlayerManager player;
    [SerializeField] PlayerSkill skill;
    [Header("Required Objects")]
    [SerializeField] AssetReferenceGameObject playerProjectile;
    [SerializeField] Transform projectileParent;
    [SerializeField] Transform shootingPos;
    [Space]
    [Header("Player Attack Info")]
    [SerializeField] float defaultAttackSpeed; // animation speed
    [SerializeField] float defaultProjectileSpeed;
    [SerializeField] float defaltProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [SerializeField] int defaultPlayerAtk = 1;
    [SerializeField] float multishotTimeInterval = .3f;

    public EnemyController CurrentTarget { get; set; }
    public float AttackSpeed { get; set; }
    public static int Atk { get; set; }
    public static float ProjectileSpeed { get; set; }


    private readonly List<ShotInstruction> shotPlan = new(10);

    public void Init() 
    {
        if (!debugMode) 
        {
            // data 가져오기
        }

        Atk = defaultPlayerAtk;
        AttackSpeed = defaultAttackSpeed;
        ProjectileSpeed = defaultProjectileSpeed;
    }

    public void Attack()
    {
        EnemyController target = GetEenemyTarget();

        if (target != null)
        {
            CurrentTarget = target;
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
    }

    private EnemyController GetEenemyTarget()
    {
        EnemyController target = null;
        float minDistance = float.MaxValue;

        if (EnemyManager.Enemies.Count <= 0)
            return null;

        foreach (EnemyController enemy in EnemyManager.Enemies)
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
        StartCoroutine(ShootingCoroutine());
    }

    IEnumerator ShootingCoroutine()
    {
        // 1) 플랜 1회만 가져오기
        var plan = GetShotPlan();
        if (plan == null || plan.Count == 0) yield break;

        // 2) 멀티샷 카운트 결정 (인터페이스 기반)
        int multishots = 1;
        if (PlayerSkill.acquiredSkillModule.TryGetValue(PlayerSkillId.MultiShot, out var mod)
            && mod is MultiShot multi)
        {
            multishots = Mathf.Max(1, multi.MultiShotCount);
        }

        // 3) 발사 간격(옵션)
        
        WaitForSeconds wait = multishotTimeInterval > 0f ? new WaitForSeconds(multishotTimeInterval) : null;

        // 4) 발사 루프
        for (int i = 0; i < multishots; i++)
        {
            List<GameObject> bulletWave = new List<GameObject>();

            for (int j = 0; j < plan.Count; j++) 
            {
                var inst = plan[j];

                GameObject go = null;
                yield return PoolManager.Instance.GetObject(playerProjectile, obj => go = obj, projectileParent);
                if (!go) continue;

                bulletWave.Add(go);

                var proj = go.GetComponent<Projectile_Player>();
                if (proj == null)
                {
                    // 풀 반납 또는 비활성 처리
                    go.SetActive(false);
                    continue;
                }

                proj.SetupProjectile(inst.shootingPos, inst.shootingDir,
                                     ProjectileSpeed, Atk, defaltProjectileLifetime, _isFlying: false);
            }

            foreach (var bullet in bulletWave) bullet.SetActive(true);

            // 마지막 샷 이후에는 대기하지 않음
            if (i < multishots - 1 && wait != null)
                yield return wait;
        }
    }

    private IReadOnlyList<ShotInstruction> GetShotPlan()
    {
        RebuildShotPlan();

        return shotPlan;
    }

    private void RebuildShotPlan()
    {
        shotPlan.Clear();
        
        Vector3 _dir = (CurrentTarget.transform.position - transform.position).normalized;
        Vector3 _pos = shootingPos.position;

        ShotInstruction _instr = new ShotInstruction { shootingDir = _dir, shootingPos = _pos };

        if (PlayerSkill.acquiredSkillModule.TryGetValue(PlayerSkillId.HorizontalShot, out var horizon))
        {
            if (horizon is IShootContributor shotMod)
            {
                shotMod.AddBullet(shotPlan, _instr);
            }
        }
        else 
        {
            shotPlan.Add(_instr);
        }

        if (PlayerSkill.acquiredSkillModule.TryGetValue(PlayerSkillId.DiagonalShot, out var diagonal)) 
        {
            if (diagonal is IShootContributor shotMod) 
            {
                shotMod.AddBullet(shotPlan, _instr);
            }
        }
    }
    #endregion

    #region Skill Methods
    public void UpdateAttackSpeed(float _modifier) 
    {
        AttackSpeed = defaultAttackSpeed * (1 +_modifier);
        player.Anim.SetFloat(AnimHashes.AttackSpeed, AttackSpeed);
    }


    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (player == null) player = GetComponent<PlayerManager>();
        if (skill == null) skill = GetComponent<PlayerSkill>();
    }
#endif
}
