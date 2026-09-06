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

    // 데미지 속성이 아닌, BarrelManager 딕셔너리 키로만 쓰이는 배럴 식별용 플래그
    Shield    = 1 << 7, // 128 - 실드 배럴 식별용
    Heal      = 1 << 8, // 256 - 체력 회복 배럴 식별용
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
            effectDataMap[flag] = EffectData.Fallback;
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
            effectDataMap[flag] = data ?? EffectData.Fallback;
        }
    }
}