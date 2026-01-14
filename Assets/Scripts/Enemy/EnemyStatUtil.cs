using System;
using Enemy;
using Map;
using UnityEngine;

namespace Enemies
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

            bool isBoss = EnemyTagUtil.Has(tag, EnemyTag.Boss);
            bool isMelee = EnemyTagUtil.Has(tag, EnemyTag.Melee);
            bool isRanged = EnemyTagUtil.Has(tag, EnemyTag.Ranged);
            bool hasShooter = EnemyTagUtil.Has(tag, EnemyTag.Shoot);
            bool hasFlyingShooter = EnemyTagUtil.Has(tag, EnemyTag.FlyingShoot);

            var st = BuildStageMultipliers(stageIndex, isBoss, map.stageGrowth);
            var mp = BuildMapMultipliers(isBoss, isMelee, isRanged, map.mapModifiers);

            ComputeBaseStats(e.@base, st, mp, ref stats, isBoss);

            ApplyProjectileStats(e, hasShooter, hasFlyingShoooter: hasFlyingShooter, st, mp, ref stats, isBoss);

            return stats;
        }

        private struct StageMulPack
        {
            public float hpStage;
            public float atkStage;
            public float bossHpPerBoss;
            public float bossAtkPerBoss;
        }

        private struct MapMulPack
        {
            public float mapHp;
            public float mapAtk;
            public float bossMapAtk;
            public float move;
            public float bossMove;
            public float projSpeed;
            public float bossProjSpeed;
            public float flyingProjSpeed;
            public float bossFlyingProjSpeed;
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
                p.bossHpPerBoss = 1f + sg.bossHpMulPerStage * bossMod;
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

            if (isBoss) m.mapHp = mm.bossEnemyHpMulPerMap > 0f ? mm.bossEnemyHpMulPerMap : 1f;
            else if (isMelee) m.mapHp = mm.meleeEnemyHpMulPerMap > 0f ? mm.meleeEnemyHpMulPerMap : 1f;
            else if (isRanged) m.mapHp = mm.rangedEnemyHpMulPerMap > 0f ? mm.rangedEnemyHpMulPerMap : 1f;

            m.mapAtk = mm.atkMulPerMap > 0f ? mm.atkMulPerMap : 1f;
            m.move = mm.moveSpeedMul > 0f ? mm.moveSpeedMul : 1f;
            m.projSpeed = mm.projectileSpeedMul > 0f ? mm.projectileSpeedMul : 1f;
            m.flyingProjSpeed = mm.flyingProjectileSpeedMul > 0f ? mm.flyingProjectileSpeedMul : 1f;

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
        public ShootingStats shooting;
        public FlyingShootingStats flyingShooting;
    };
}
