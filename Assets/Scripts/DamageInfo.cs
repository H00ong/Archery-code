using UnityEngine;

// 데미지 속성 정의
[System.Flags]
public enum EffectType
{
    Normal = 1 << 0,    // 1
    Fire = 1 << 1,    // 2
    Poison = 1 << 2,    // 4
    Ice = 1 << 3,    // 8
    Lightning = 1 << 4,    // 16
    Magma = 1 << 5,    // 32
    Dark = 1 << 6,    // 64
}

[System.Serializable]
public class DamageInfo
{
    public float damageAmount;            // 데미지 양
    public EffectType type;         // 속성 타입
    public GameObject attackSource;       // 데미지를 준 객체
    
    // 상태 이상 관련 파라미터 (필요 시 사용)
    public float effectDuration;    // 지속 시간 (불, 독, 얼음 지속시간)
    public float effectValue;       // 효과 강도 (독 데미지 틱당 양, 얼음 슬로우 비율 등)

    // 생성자
    public DamageInfo(float amount, EffectType type, GameObject source = null)
    {
        this.damageAmount = amount;
        this.type = type;
        this.attackSource = source;

        this.effectDuration = 3.0f; // 기본 지속 시간 3초
        this.effectValue = 0.5f;    // 기본 값 (상황에 따라 다름)
    }
}
