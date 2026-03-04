using System;
using System.Collections.Generic;
using Stat;

namespace Enemy
{
    [Serializable]
    public class ShootingStats
    {
        public int projectileAtk;
        public float projectileSpeed;
    }

    [Serializable]
    public class FlyingShootingStats
    {
        public int flyingProjectileAtk;
        public float flyingProjectileSpeed;
    }

    /// <summary>
    /// 적 전용 스탯 클래스. BaseStat을 상속받아 공통 인터페이스를 구현한다.
    /// 몬스터는 장비·버프 시스템 없이 기본값 위주로 사용하되, 추후 확장 가능하다.
    /// Armor, MagicResistance도 BaseStat을 통해 자연스럽게 보유한다.
    /// </summary>
    [Serializable]
    public class EnemyStat : BaseStat
    {
        // ========== 기본 스탯 (BaseStat 구현) ==========
        private int _maxHP;                 // 최대 체력
        private int _attackPower;           // 공격력
        private float _moveSpeed;           // 이동 속도
        private int _armor;                 // 물리 방어력 (추후 확장)
        private int _magicResistance;       // 마법 저항력 (추후 확장)
        private EffectType _attackEffectType; // 공격 속성 타입 (Flags - 복수 가능)

        // ========== Enemy 전용: 투사체 스탯 ==========
        public ShootingStats shooting = new();
        public FlyingShootingStats flyingShooting = new();

        // ========== EffectType별 상태이상 데이터 (EnemyManager에서 캐싱) ==========
        private Dictionary<EffectType, EffectData> _effectDataMap = new();

        // ========== BaseStat 구현 ==========
        public override int MaxHP => _maxHP;
        public override int AttackPower => _attackPower;
        public override float MoveSpeed => _moveSpeed;
        public override int Armor => _armor;
        public override int MagicResistance => _magicResistance;
        public override EffectType AttackEffectType => _attackEffectType;

        /// <summary>
        /// 해당 EffectType에 대응하는 EffectData를 반환한다.
        /// 데이터가 없으면 null을 반환한다.
        /// </summary>
        public override EffectData GetEffectData(EffectType type)
        {
            return _effectDataMap.TryGetValue(type, out var data) ? data : null;
        }

        // ========== Setters (EnemyStatUtil에서 호출) ==========

        /// <summary> 기본 스탯 설정 (hp, atk, moveSpeed, armor, magicResistance, attackEffectType) </summary>
        public void SetBaseStats(int hp, int atk, float moveSpeed, int armor, int magicResistance, EffectType attackEffectType = EffectType.Normal)
        {
            _maxHP = hp;
            _attackPower = atk;
            _moveSpeed = moveSpeed;
            _armor = armor;
            _magicResistance = magicResistance;
            _attackEffectType = attackEffectType;
        }

        /// <summary> EffectData 설정 (EnemyManager에서 캐싱된 값 주입) </summary>
        public void SetEffectData(EffectType type, EffectData data)
        {
            _effectDataMap[type] = data;
        }

        /// <summary> EffectData 맵 전체 설정 </summary>
        public void SetEffectDataMap(Dictionary<EffectType, EffectData> map)
        {
            _effectDataMap = map ?? new Dictionary<EffectType, EffectData>();
        }

        public void SetAttackEffectType(EffectType type)
        {
            _attackEffectType = type;
        }
    }
}