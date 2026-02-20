using System.Collections.Generic;
using Stat;
using UnityEngine;

// 데미지 속성 정의
[System.Flags]
public enum EffectType
{
    Normal    = 1 << 0, // 1
    Fire      = 1 << 1, // 2
    Poison    = 1 << 2, // 4
    Ice       = 1 << 3, // 8
    Lightning = 1 << 4, // 16
    Magma     = 1 << 5, // 32
    Dark      = 1 << 6, // 64
}

[System.Serializable]
public class DamageInfo
{
    public float damageAmount;      // 데미지 양
    public EffectType type;         // 속성 타입 (Flags - 복수 가능)
    public GameObject attackSource; // 데미지를 준 객체

    // EffectType별 상태이상 파라미터
    public Dictionary<EffectType, EffectData> effectDataMap;

    // 특정 단일 EffectType의 EffectData 반환 (없으면 null)
    public EffectData GetEffectData(EffectType singleType)
    {
        effectDataMap.TryGetValue(singleType, out var data);
        return data;
    }

    // 기본 생성자 - 각 플래그별 기본값으로 초기화
    public DamageInfo(float amount, EffectType type, GameObject source = null)
    {
        this.damageAmount  = amount;
        this.type          = type;
        this.attackSource  = source;
        this.effectDataMap = new Dictionary<EffectType, EffectData>();

        foreach (var flag in Utils.AllEffectTypes)
            effectDataMap[flag] = new EffectData(3.0f, 0.3f, 1f, 0.3f);
    }

    // Stat 기반 생성자 - 공격자의 BaseStat에서 EffectType별 EffectData를 읽어 초기화
    public DamageInfo(float amount, EffectType type, BaseStat attackerStat, GameObject source = null)
    {
        this.damageAmount  = amount;
        this.type          = type;
        this.attackSource  = source;
        this.effectDataMap = new Dictionary<EffectType, EffectData>();

        foreach (var flag in Utils.AllEffectTypes)
        {
            var data = attackerStat?.GetEffectData(flag);
            effectDataMap[flag] = data ?? new EffectData(3.0f, 0.3f, 1f, 0.3f);
        }
    }
}