using System;
using System.Collections.Generic;
using Map;
using Stat;
using UnityEngine;

namespace Enemy
{
    public static class EnemyStatUtil
    {
        public static void CalculateStat(
            EnemyStat outStat, EnemyData e, EnemyTag tag, MapData map, int stageIndex)
        {
            if (e == null)
            {
                Debug.LogError($"[EnemyStatUtil] EnemyData is null — no JSON entry matched (name lookup failed, tag={tag})");
                return;
            }
            if (map == null)
            {
                Debug.LogError($"[EnemyStatUtil] MapData is null — CurrentMapData not loaded yet (enemy={e.enemyName})");
                return;
            }
            if (e.@base == null)
            {
                Debug.LogError($"[EnemyStatUtil] EnemyData.base is null — JSON 'base' field missing for {e.enemyName}");
                return;
            }

            bool isBoss = EnemyTagUtil.Has(tag, EnemyTag.Boss);
            bool isMelee = EnemyTagUtil.Has(tag, EnemyTag.Melee);
            bool isRanged = EnemyTagUtil.Has(tag, EnemyTag.Ranged);
            bool hasShooter = EnemyTagUtil.Has(tag, EnemyTag.Shoot);
            bool hasFlyingShooter = EnemyTagUtil.Has(tag, EnemyTag.FlyingShoot);

            var st = BuildStageMultipliers(stageIndex, isBoss, map.stageGrowth);
            var mp = BuildMapMultipliers(isBoss, isMelee, isRanged, map.mapModifiers);

            ComputeBaseStats(e.@base, st, mp, outStat, isBoss);
            outStat.SetAttackEffectType(EnemyTagUtil.ToEffectType(tag));
            outStat.SetEffectDataMap(EnemyEffectCache.GetAll());
            ApplyProjectileStats(e, hasShooter, hasFlyingShoooter: hasFlyingShooter, st, mp, outStat, isBoss);
        }

        private struct StageMulPack
        {
            public float hpStage;
            public float atkStage;
            public float bossHpStage;
            public float bossAtkStage;
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
            var p = new StageMulPack { hpStage = 1f, atkStage = 1f, bossHpStage = 1f, bossAtkStage = 1f };
            if (sg == null)
                return p;

            // 스테이지가 0일 때 체력이 0이 되는 것을 방지하기 위해 최소 1로 설정합니다.
            int stage = Mathf.Max(1, stageIndex);

            // 일반 몬스터: 1.1 입력 시 1.1 -> 1.21 -> 1.33 순으로 누적되도록 거듭제곱(복리) 적용
            p.hpStage = sg.hpMulPerStage > 1f ? Mathf.Pow(sg.hpMulPerStage, stage) : 1f;
            p.atkStage = sg.atkMulPerStage > 1f ? Mathf.Pow(sg.atkMulPerStage, stage) : 1f;

            if (isBoss)
            {
                // 보스 몬스터: 요청하신 [bossHpMulPerStage * Stage] 공식 적용
                // (기본 스탯과 bossMapHp는 ComputeBaseStats에서 곱해집니다)
                p.bossHpStage = sg.bossHpMulPerStage > 1f ? sg.bossHpMulPerStage * stage : 1f;
                p.bossAtkStage = sg.bossAtkMulPerStage > 1f ? sg.bossAtkMulPerStage * stage : 1f;
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
            if (mm == null)
                return m;

            // 모든 배율 조건을 > 1f 로 변경
            if (isBoss)
                m.mapHp = mm.bossEnemyHpMulPerMap > 1f ? mm.bossEnemyHpMulPerMap : 1f;
            else if (isMelee)
                m.mapHp = mm.meleeEnemyHpMulPerMap > 1f ? mm.meleeEnemyHpMulPerMap : 1f;
            else if (isRanged)
                m.mapHp = mm.rangedEnemyHpMulPerMap > 1f ? mm.rangedEnemyHpMulPerMap : 1f;

            m.mapAtk = mm.atkMulPerMap > 1f ? mm.atkMulPerMap : 1f;
            m.move = mm.moveSpeedMul > 1f ? mm.moveSpeedMul : 1f;
            m.projSpeed = mm.projectileSpeedMul > 1f ? mm.projectileSpeedMul : 1f;
            m.flyingProjSpeed = mm.flyingProjectileSpeedMul > 1f ? mm.flyingProjectileSpeedMul : 1f;

            m.bossMapAtk = mm.bossAtkMulPerMap > 1f ? mm.bossAtkMulPerMap : m.mapAtk;
            m.bossMove = mm.bossMoveSpeedMulPerMap > 1f ? mm.bossMoveSpeedMulPerMap : m.move;
            m.bossProjSpeed = mm.bossProjectileSpeedMulPerMap > 1f ? mm.bossProjectileSpeedMulPerMap : m.projSpeed;
            m.bossFlyingProjSpeed = mm.bossFlyingProjectileSpeedMulPerMap > 1f ? mm.bossFlyingProjectileSpeedMulPerMap : m.flyingProjSpeed;

            return m;
        }

        private static void ComputeBaseStats(EnemyBase b, in StageMulPack st, in MapMulPack mp, EnemyStat outStats, bool isBoss = false)
        {
            // 보스일 경우 일반 배율을 타지 않고 보스 전용 Stage 배율로 덮어씁니다.
            float hpStage = isBoss ? st.bossHpStage : st.hpStage;
            float atkStage = isBoss ? st.bossAtkStage : st.atkStage;

            // 보스 최종 HP = b.hp(기본) * st.bossHpStage(보스배율 x Stage) * mp.mapHp(맵배율)
            float hp = b.hp * hpStage * mp.mapHp;
            float atk = b.atk * atkStage * (isBoss ? mp.bossMapAtk : mp.mapAtk);
            float ms = b.moveSpeed * (isBoss ? mp.bossMove : mp.move);

            outStats.SetBaseStats(Mathf.RoundToInt(hp), Mathf.RoundToInt(atk), ms, b.armor, b.magicResistance);
        }

        private static void ApplyProjectileStats(
            EnemyData e, bool hasShooter, bool hasFlyingShoooter,
            in StageMulPack st, in MapMulPack mp, EnemyStat outStats, bool isBoss = false)
        {
            float atkStage = isBoss ? st.bossAtkStage : st.atkStage;
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
}