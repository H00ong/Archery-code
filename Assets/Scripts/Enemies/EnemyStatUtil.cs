using Game.Enemies.Enum;
using System;
using UnityEngine;

namespace Game.Enemies.Stat
{
    public static class EnemyStatUtil
    {
        public static EnemyStats GetEnemyStats(EnemyData e, EnemyTag tag, MapData map, int stageIndex)
        {
            var stats = new EnemyStats();
            if (e == null || map == null || e.@base == null)
            {
                Debug.LogError("Enemy data or map data (or base) is null");
                return stats;
            }

            // 태그 캐시
            bool isBoss = EnemyTagUtil.Has(tag, EnemyTag.Boss);
            bool isMelee = EnemyTagUtil.Has(tag, EnemyTag.Melee);
            bool isRanged = EnemyTagUtil.Has(tag, EnemyTag.Ranged); // 없으면 제거
            bool hasShooter = EnemyTagUtil.Has(tag, EnemyTag.Shoot);
            bool hasFlyingShooter = EnemyTagUtil.Has(tag, EnemyTag.FlyingShoot);

            // 1) 배수 계산
            var st = BuildStageMultipliers(stageIndex, isBoss, map.stageGrowth);
            var mp = BuildMapMultipliers(isBoss, isMelee, isRanged, map.mapModifiers);

            // 2) 기본 스탯
            ComputeBaseStats(e.@base, st, mp, ref stats, isBoss);

            // 3) 투사체(있을 때만)
            ApplyProjectileStats(e, hasShooter, hasFlyingShoooter: hasFlyingShooter, st, mp, ref stats, isBoss);

            return stats;
        }

        /* ----------------- 내부 헬퍼들 ----------------- */

        private struct StageMulPack
        {
            public float hpStage;  // 1 + hpPerStage*(stage-1)
            public float atkStage; // 1 + atkPerStage*(stage-1)
            public float bossHpPerBoss;  // 보스 순번 성장(없으면 1)
            public float bossAtkPerBoss; // 보스 순번 성장(없으면 1)
        }

        private struct MapMulPack
        {
            public float mapHp;            // 일반/보스별 HP 맵 배수
            public float mapAtk;           // 일반 ATK 맵 배수
            public float bossMapAtk;       // 보스 ATK 맵 배수(없으면 mapAtk)
            public float move;             // 일반 이동속도
            public float bossMove;         // 보스 이동속도
            public float projSpeed;        // 일반 투사체 속도
            public float bossProjSpeed;    // 보스 투사체 속도
            public float flyingProjSpeed;  // 일반 비행 투사체 속도
            public float bossFlyingProjSpeed; // 보스 비행 투사체 속도
        }

        private static StageMulPack BuildStageMultipliers(int stageIndex, bool isBoss, StageGrowth sg)
        {
            var p = new StageMulPack { hpStage = 1f, atkStage = 1f, bossHpPerBoss = 1f, bossAtkPerBoss = 1f };
            if (sg == null) return p;

            int stage = Mathf.Max(0, stageIndex);
            int defaultMod = stage;
            int bossMod = defaultMod / 10;

            p.hpStage = 1f + sg.hpMulPerStage * defaultMod;
            p.atkStage = 1f + sg.atkMulPerStage * defaultMod;

            if (isBoss)
            {
                p.bossHpPerBoss = 1f + sg.bossHPMulPerStage * bossMod;
                p.bossAtkPerBoss = 1f + sg.bossAtkMulPerStage * bossMod;
            }
            return p;
        }

        private static MapMulPack BuildMapMultipliers(bool isBoss, bool isMelee, bool isRanged, MapModifiers mm)
        {
            var m = new MapMulPack
            {
                mapHp = 1f,
                mapAtk = 1f,
                bossMapAtk = 1f,
                move = 1f,
                bossMove = 1f,
                projSpeed = 1f,
                bossProjSpeed = 1f,
                flyingProjSpeed = 1f,
                bossFlyingProjSpeed = 1f
            };
            if (mm == null) return m;

            // HP 맵 배수
            if (isBoss) m.mapHp = mm.bossEnemyHpMulPerMap > 0f ? mm.bossEnemyHpMulPerMap : 1f;
            else if (isMelee) m.mapHp = mm.meleeEnemyHpMulPerMap > 0f ? mm.meleeEnemyHpMulPerMap : 1f;
            else if (isRanged) m.mapHp = mm.rangedEnemyHpMulPerMap > 0f ? mm.rangedEnemyHpMulPerMap : 1f;

            // ATK, 이동, 투사체 속도
            m.mapAtk = mm.atkMulPerMap > 0f ? mm.atkMulPerMap : 1f;
            m.move = mm.moveSpeedMul > 0f ? mm.moveSpeedMul : 1f;
            m.projSpeed = mm.projectileSpeedMul > 0f ? mm.projectileSpeedMul : 1f;
            m.flyingProjSpeed = mm.flyingProjectileSpeedMul > 0f ? mm.flyingProjectileSpeedMul : 1f;

            // 보스용(없으면 일반값 사용)
            m.bossMapAtk = mm.bossAtkMulPerMap > 0f ? mm.bossAtkMulPerMap : m.mapAtk;
            m.bossMove = mm.bossMoveSpeedMulPerMap > 0f ? mm.bossMoveSpeedMulPerMap : m.move;
            m.bossProjSpeed = mm.bossProjectileSpeedMulPerMap > 0f ? mm.bossProjectileSpeedMulPerMap : m.projSpeed;
            m.bossFlyingProjSpeed = mm.bossFlyingProjectileSpeedMulPerMap > 0f ? mm.bossFlyingProjectileSpeedMulPerMap : m.flyingProjSpeed;

            return m;
        }

        private static void ComputeBaseStats(EnemyBase b, in StageMulPack st, in MapMulPack mp, ref EnemyStats outStats, bool isBoss = false)
        {
            float hpStage = isBoss ? st.hpStage * st.bossHpPerBoss : st.hpStage;
            float atkStage = isBoss ? st.atkStage * st.bossAtkPerBoss : st.atkStage;

            float hp = b.hp * hpStage * mp.mapHp;
            float atk = b.atk * atkStage * (isBoss ? mp.bossMapAtk : mp.mapAtk);
            float ms = b.moveSpeed * (isBoss ? mp.bossMove : mp.move);

            outStats.baseStats.hp = Mathf.RoundToInt(hp);
            outStats.baseStats.atk = Mathf.RoundToInt(atk);
            outStats.baseStats.moveSpeed = ms;
        }

        private static void ApplyProjectileStats(
            EnemyData e, bool hasShooter, bool hasFlyingShoooter,
            in StageMulPack st, in MapMulPack mp, ref EnemyStats outStats, bool isBoss = false)
        {
            float atkStage = isBoss ? st.atkStage * st.bossAtkPerBoss : st.atkStage;
            float atkMap = isBoss ? mp.bossMapAtk : mp.mapAtk;

            if (hasShooter && e.shooter != null)
            {
                outStats.shooting.projectileSpeed = e.shooter.projectileSpeed * (isBoss ? mp.bossProjSpeed : mp.projSpeed);
                float pAtk = e.shooter.projectileAtk * atkStage * atkMap;
                outStats.shooting.projectileAtk = Mathf.RoundToInt(pAtk);
            }
            if (hasFlyingShoooter && e.flyingShooter != null)
            {
                outStats.flyingShooting.flyingProjectileSpeed =
                    e.flyingShooter.flyingProjectileSpeed * (isBoss ? mp.bossFlyingProjSpeed : mp.flyingProjSpeed);
                float fpAtk = e.flyingShooter.flyingProjectileAtk * atkStage * atkMap;
                outStats.flyingShooting.flyingProjectileAtk = Mathf.RoundToInt(fpAtk);
            }
        }
    }


    [Serializable]
    public struct BaseStats
    {
        public int hp, atk;
        public float moveSpeed;
    };

    [Serializable]
    public struct ShootingStats
    {
        public int projectileAtk;
        public float projectileSpeed;
    };
    [Serializable]
    public struct FlyingShootingStats
    {
        public int flyingProjectileAtk;
        public float flyingProjectileSpeed;
    };

    [Serializable]
    public struct EnemyStats
    {
        public BaseStats baseStats;
        public ShootingStats shooting;            // hasShooting == false면 무시
        public FlyingShootingStats flyingShooting;        // hasFlying == false면 무시
    };
}
