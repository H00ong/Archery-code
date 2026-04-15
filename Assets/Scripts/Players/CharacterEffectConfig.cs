using System;
using Stat;

namespace Players
{
    /// <summary>
    /// Inspector에서 캐릭터별 EffectType + EffectData를 설정하는 직렬화 가능 구조체.
    /// CharacterIdentity에서 배열로 사용된다.
    /// </summary>
    [Serializable]
    public struct CharacterEffectConfig
    {
        public EffectType effectType;
        public EffectData baseEffect;
    }

    /// <summary>
    /// 레벨업 시 EffectData 성장치. CharacterIdentity에서 배열로 사용된다.
    /// effectType이 동일한 CharacterEffectConfig와 매칭되어 레벨당 성장 적용.
    /// </summary>
    [Serializable]
    public struct CharacterEffectGrowth
    {
        public EffectType effectType;
        public float durationGrowth;
        public float valueGrowth;
        public float dotDamageGrowth;
        public float tickIntervalGrowth;
    }
}
