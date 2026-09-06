using System;
using UnityEngine;

namespace Stat
{
    /// <summary>
    /// 모든 캐릭터(Player, Enemy)의 공통 스탯 베이스 클래스.
    /// 공통 필드: MaxHP, AttackPower, MoveSpeed, Armor, MagicResistance
    /// EffectType별 상태이상 데이터(EffectData)도 보유한다.
    /// </summary>
    [Serializable]
    public abstract class BaseStat : MonoBehaviour
    {
        /// <summary> 최대 체력 </summary>
        public abstract int MaxHP { get; }

        /// <summary> 공격력 </summary>
        public abstract int AttackPower { get; }

        /// <summary> 이동 속도 </summary>
        public abstract float MoveSpeed { get; }

        /// <summary> 물리 방어력 </summary>
        public abstract int Armor { get; }

        /// <summary> 마법 저항력 </summary>
        public abstract int MagicResistance { get; }

        public abstract EffectType AttackEffectType { get; }

        /// <summary>
        /// 스킬 버프를 제외한, 캐릭터·장비에서 타고난 공격 속성.
        /// 속성 부여 스킬(AttackGrant)의 등장 조건 판정에 사용된다.
        /// </summary>
        public virtual EffectType InnateAttackEffectType => AttackEffectType;

        /// <summary>
        /// 해당 EffectType에 대응하는 최종 EffectData를 반환한다.
        /// 데이터가 없으면 null을 반환한다.
        /// </summary>
        public abstract EffectData GetEffectData(EffectType type);
    }
}
