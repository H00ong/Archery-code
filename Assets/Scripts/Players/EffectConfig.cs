using System;
using Stat;

namespace Players
{
    /// <summary>
    /// 캐릭터·장비 공용 이펙트 기본 설정 구조체.
    /// CharacterIdentity/EquipmentIdentity의 effectConfigs 배열에 사용된다.
    /// </summary>
    [Serializable]
    public struct EffectConfig
    {
        public EffectType effectType;
        public EffectData baseEffect;
    }

    /// <summary>
    /// 캐릭터·장비 공용 이펙트 레벨 성장치 구조체.
    /// CharacterIdentity/EquipmentIdentity의 effectGrowths 배열에 사용된다.
    /// effectType이 동일한 EffectConfig와 매칭되어 레벨당 성장 적용.
    /// </summary>
    [Serializable]
    public struct EffectGrowth
    {
        public EffectType effectType;
        public float durationGrowth;
        public float valueGrowth;
        public float dotDamageGrowth;
        public float tickIntervalGrowth;
    }
}
