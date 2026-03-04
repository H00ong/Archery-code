using System;

namespace Stat
{
    /// <summary>
    /// 각 EffectType에 대응하는 상태이상 효과 데이터.
    /// duration, value, dotDamage, tickInterval 등을 보유한다.
    /// 3-Layer 합산(Base + Equipment + Buff)에 사용할 수 있도록 + 연산자를 지원한다.
    /// </summary>
    [Serializable]
    public class EffectData
    {
        public float duration;      // 지속 시간 (초)
        public float value;         // 효과 강도 (슬로우 %, 화상 배율 등)
        public float dotDamage;     // 도트 데미지 양
        public float tickInterval;  // 도트 데미지 틱 간격

        public EffectData()
        {
            duration = 0f;
            value = 0f;
            dotDamage = 0f;
            tickInterval = 0f;
        }

        public EffectData(float duration, float value, float dotDamage, float tickInterval)
        {
            this.duration = duration;
            this.value = value;
            this.dotDamage = dotDamage;
            this.tickInterval = tickInterval;
        }

        /// <summary> 기본값 (아무 효과 없음) </summary>
        public static EffectData Zero => new(0f, 0f, 0f, 0f);

        /// <summary>
        /// 두 EffectData를 합산한다.
        /// tickInterval은 base 값을 유지(스택 불가)한다.
        /// </summary>
        public static EffectData operator +(EffectData a, EffectData b)
        {
            return new EffectData(
                a.duration + b.duration,
                a.value + b.value,
                a.dotDamage + b.dotDamage,
                a.tickInterval > 0f ? a.tickInterval : b.tickInterval
            );
        }

        public EffectData Clone()
        {
            return new EffectData(duration, value, dotDamage, tickInterval);
        }
    }
}
