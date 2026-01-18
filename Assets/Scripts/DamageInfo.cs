using UnityEngine;

// 데미지 속성 정의
public enum DamageType
{
    Normal,
    Fire,   // 지속 데미지 (짧고 강함)
    Venom,  // 지속 데미지 (길고 약함)
    Ice     // 애니메이션/이동 속도 저하
}

[System.Serializable]
public class DamageInfo
{
    public float amount;            // 데미지 양
    public DamageType type;         // 속성 타입
    public GameObject instigator;   // 공격한 대상 (누가 때렸는지, null 가능)
    public Vector3 hitPoint;        // 타격 위치 (파티클 생성용)
    
    // 상태 이상 관련 파라미터 (필요 시 사용)
    public float effectDuration;    // 지속 시간 (불, 독, 얼음 지속시간)
    public float effectValue;       // 효과 강도 (독 데미지 틱당 양, 얼음 슬로우 비율 등)

    // 생성자
    public DamageInfo(float amount, DamageType type, GameObject instigator = null)
    {
        this.amount = amount;
        this.type = type;
        this.instigator = instigator;
        this.effectDuration = 3.0f; // 기본 지속 시간 3초
        this.effectValue = 0.5f;    // 기본 값 (상황에 따라 다름)
    }
}
