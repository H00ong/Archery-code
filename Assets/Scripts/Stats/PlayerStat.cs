using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stat
{
    /// <summary>
    /// 플레이어 전용 스탯 클래스. 3단 레이어(Layer) 시스템:
    /// 
    /// A. Base        - 캐릭터 고유 기본 능력치
    /// B. Equipment   - 로비에서 장착한 악세서리 등에 의한 영구적 보정치 (게임 재시작 시 유지)
    /// C. InGameBuff  - 게임 플레이 중 획득한 일시적 보정 (게임 종료 시 0으로 초기화)
    /// 
    /// 최종값 = Base + Equipment + InGameBuff
    /// 예: TotalAttack = BaseAttack + EquipmentAttack + InGameBuffAttack
    /// </summary>
    [Serializable]
    public class PlayerStat : BaseStat
    {
        // ================================================================
        // A. Base Layer (기본 능력치) — 캐릭터 고유 기본값
        // ================================================================
        [Header("Base — 캐릭터 고유 기본 능력치")]
        [SerializeField] private int baseMaxHP = 100;           // 기본 최대 체력
        [SerializeField] private int baseAttackPower = 1;       // 기본 공격력
        [SerializeField] private float baseMoveSpeed = 5f;      // 기본 이동 속도
        [SerializeField] private int baseArmor;                 // 기본 물리 방어력
        [SerializeField] private int baseMagicResistance;       // 기본 마법 저항력
        [SerializeField] private float baseAttackSpeed = 1f;    // 기본 공격 속도 (애니메이션)
        [SerializeField] private float baseProjectileSpeed = 10f; // 기본 투사체 속도

        [Header("Base — 기본 공격 속성")]
        [SerializeField] private EffectType attackEffectType = EffectType.Normal; // 기본 공격 속성

        // ================================================================
        // B. Equipment Layer (장비 보정치) — 로비에서 장착, 게임 재시작 시 유지
        // ================================================================
        private int equipMaxHP;             // 장비 최대 체력 보정
        private int equipAttackPower;       // 장비 공격력 보정
        private float equipMoveSpeed;       // 장비 이동 속도 보정
        private int equipArmor;             // 장비 물리 방어력 보정
        private int equipMagicResistance;   // 장비 마법 저항력 보정
        private float equipAttackSpeed;     // 장비 공격 속도 보정
        private float equipProjectileSpeed; // 장비 투사체 속도 보정
        private EffectType equipAttackEffectType; // 장비 공격 속성 보정 (추가 속성)

        // ================================================================
        // C. InGame Buff Layer (인게임 버프) — 게임 종료 시 0으로 초기화
        // ================================================================
        private int buffMaxHP;              // 버프 최대 체력 보정
        private int buffAttackPower;        // 버프 공격력 보정
        private float buffMoveSpeed;        // 버프 이동 속도 보정
        private int buffArmor;              // 버프 물리 방어력 보정
        private int buffMagicResistance;    // 버프 마법 저항력 보정
        private float buffAttackSpeed;      // 버프 공격 속도 보정
        private float buffProjectileSpeed;  // 버프 투사체 속도 보정
        private EffectType buffAttackEffectType; // 버프 공격 속성 보정 (추가 속성)

        // ================================================================
        //  EffectData 3-Layer (EffectType별 상태이상 효과 데이터)
        //  Base: 캐릭터 고유 기본 효과값
        //  Equipment: 악세서리 등에 의한 보정
        //  Buff: 인게임 버프에 의한 보정
        // ================================================================
        private readonly Dictionary<EffectType, EffectData> _baseEffectDataMap = new();
        private readonly Dictionary<EffectType, EffectData> _equipEffectDataMap = new();
        private readonly Dictionary<EffectType, EffectData> _buffEffectDataMap = new();

        // ================================================================
        //  최종 합산 Properties  (Total = Base + Equipment + InGameBuff)
        // ================================================================

        /// <summary> 최종 최대 체력 = Base + Equipment + Buff </summary>
        public override int MaxHP => baseMaxHP + equipMaxHP + buffMaxHP;

        /// <summary> 최종 공격력 = Base + Equipment + Buff </summary>
        public override int AttackPower => baseAttackPower + equipAttackPower + buffAttackPower;

        /// <summary> 최종 이동 속도 = Base + Equipment + Buff </summary>
        public override float MoveSpeed => baseMoveSpeed + equipMoveSpeed + buffMoveSpeed;

        /// <summary> 최종 물리 방어력 = Base + Equipment + Buff </summary>
        public override int Armor => baseArmor + equipArmor + buffArmor;

        /// <summary> 최종 마법 저항력 = Base + Equipment + Buff </summary>
        public override int MagicResistance => baseMagicResistance + equipMagicResistance + buffMagicResistance;

        /// <summary> 최종 공격 속도 = Base + Equipment + Buff </summary>
        public float AttackSpeed => baseAttackSpeed + equipAttackSpeed + buffAttackSpeed;

        /// <summary> 최종 투사체 속도 = Base + Equipment + Buff </summary>
        public float ProjectileSpeed => baseProjectileSpeed + equipProjectileSpeed + buffProjectileSpeed;

        /// <summary> 기본 공격 속성 타입 (Fire, Ice 등) </summary>
        public override EffectType AttackEffectType => attackEffectType | equipAttackEffectType | buffAttackEffectType;

        // ================================================================
        //  EffectData — 최종 합산 (Base + Equipment + Buff)
        // ================================================================

        /// <summary>
        /// 해당 EffectType에 대응하는 최종 EffectData(Base + Equipment + Buff)를 반환한다.
        /// 어떤 레이어에도 데이터가 없으면 null을 반환한다.
        /// </summary>
        public override EffectData GetEffectData(EffectType type)
        {
            bool hasBase = _baseEffectDataMap.TryGetValue(type, out var baseData);
            bool hasEquip = _equipEffectDataMap.TryGetValue(type, out var equipData);
            bool hasBuff = _buffEffectDataMap.TryGetValue(type, out var buffData);

            if (!hasBase && !hasEquip && !hasBuff)
                return null;

            var result = baseData ?? EffectData.Zero;
            if (hasEquip) result += equipData;
            if (hasBuff) result += buffData;

            return result;
        }

        // ================================================================
        //  Equipment Setters (장비 보정치 설정)
        // ================================================================

        public void SetEquipMaxHP(int value)             => equipMaxHP = value;
        public void SetEquipAttackPower(int value)       => equipAttackPower = value;
        public void SetEquipMoveSpeed(float value)       => equipMoveSpeed = value;
        public void SetEquipArmor(int value)             => equipArmor = value;
        public void SetEquipMagicResistance(int value)   => equipMagicResistance = value;
        public void SetEquipAttackSpeed(float value)     => equipAttackSpeed = value;
        public void SetEquipProjectileSpeed(float value) => equipProjectileSpeed = value;
        public void SetEquipAttackEffectType(EffectType type) => equipAttackEffectType |= type;
        // ================================================================
        //  Buff Setters (인게임 버프 직접 설정)
        // ================================================================

        public void SetBuffMaxHP(int value)              => buffMaxHP = value;
        public void SetBuffAttackPower(int value)        => buffAttackPower = value;
        public void SetBuffMoveSpeed(float value)        => buffMoveSpeed = value;
        public void SetBuffArmor(int value)              => buffArmor = value;
        public void SetBuffMagicResistance(int value)    => buffMagicResistance = value;
        public void SetBuffAttackSpeed(float value)      => buffAttackSpeed = value;
        public void SetBuffProjectileSpeed(float value) => buffProjectileSpeed = value;
        public void SetBuffAttackEffectType(EffectType type) => buffAttackEffectType |= type;

        // ================================================================
        //  EffectData Setters (EffectType별 상태이상 효과 설정)
        // ================================================================

        /// <summary> 기본 EffectData 설정 (캐릭터 고유 기본값) </summary>
        public void SetBaseEffectData(EffectType type, EffectData data)
            => _baseEffectDataMap[type] = data;

        /// <summary> 장비(악세서리) EffectData 보정치 설정 </summary>
        public void SetEquipEffectData(EffectType type, EffectData data)
            => _equipEffectDataMap[type] = data;

        /// <summary> 인게임 버프 EffectData 보정치 설정 </summary>
        public void SetBuffEffectData(EffectType type, EffectData data)
            => _buffEffectDataMap[type] = data;

        // ================================================================
        //  Modifier Methods (기존 스킬 시스템 호환)
        // ================================================================

        /// <summary>
        /// 이동 속도 modifier 적용. buffMoveSpeed = baseMoveSpeed × modifier.
        /// 예: modifier = 0.4 → 최종 MoveSpeed = baseMoveSpeed × 1.4
        /// (기존 defaultMoveSpeed * (1 + modifier) 공식과 동일한 결과)
        /// </summary>
        public void ApplyMoveSpeedModifier(float modifier)
        {
            buffMoveSpeed = baseMoveSpeed * modifier;
        }

        /// <summary>
        /// 공격 속도 modifier 적용. buffAttackSpeed = baseAttackSpeed × modifier.
        /// 예: modifier = 0.2 → 최종 AttackSpeed = baseAttackSpeed × 1.2
        /// (기존 defaultAttackSpeed * (1 + modifier) 공식과 동일한 결과)
        /// </summary>
        public void ApplyAttackSpeedModifier(float modifier)
        {
            buffAttackSpeed = baseAttackSpeed * modifier;
        }

        // ================================================================
        //  Reset (게임 종료 시 InGame Buff 초기화)
        // ================================================================

        /// <summary>
        /// 게임 종료 시 InGame Buff 수치만 초기화.
        /// Equipment 보정치는 유지된다.
        /// </summary>
        public void ResetInGameStats()
        {
            buffMaxHP = 0;
            buffAttackPower = 0;
            buffMoveSpeed = 0;
            buffArmor = 0;
            buffMagicResistance = 0;
            buffAttackSpeed = 0;
            buffProjectileSpeed = 0;
            buffAttackEffectType = EffectType.Normal;
            _buffEffectDataMap.Clear();
        }
    }
}