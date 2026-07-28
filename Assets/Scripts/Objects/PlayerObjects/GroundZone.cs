using System.Collections;
using System.Collections.Generic;
using Objects;
using Players;
using Stat;
using UnityEngine;

/// <summary>
/// 장판(GroundZone): 바닥에 고정된 AOE 지역.
/// 트리거 안에 있는 적에게 분당 데미지(DPS)를 지속적으로 부여한다.
/// </summary>
public class GroundZone : SceneObject
{
    // 장판이 지속될 시간 (GroundZoneSkill에서 Initialize 시 설정)
    private float _duration;

    // 분당 데미지 (AttackPower 배수)
    private float _damageMultiplierPerMinute;

    // 틱 간격 (초)
    private float _tickInterval;
    private float _tickTimer;

    // 피해 정보 (틱마다 damageAmount만 갱신하여 재사용)
    private DamageInfo _damageInfo;
    private BaseStat _playerStat;

    // 현재 장판 안에 있는 적 목록
    private readonly HashSet<IDamageable> _enemiesInZone = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        _enemiesInZone.Clear();
        _tickTimer = 0f;
    }

    /// <summary>
    /// 장판 초기화. GroundZoneSkill에서 Instantiate 직후 호출.
    /// </summary>
    /// <param name="playerStat">플레이어 스탯 (공격력 참조용)</param>
    /// <param name="damageMultiplierPerMinute">분당 데미지 = AttackPower × 이 값</param>
    /// <param name="effectType">속성 타입</param>
    /// <param name="tickInterval">틱 간격 (초)</param>
    /// <param name="duration">장판 지속 시간 (초)</param>
    public void Initialize(BaseStat playerStat, float damageMultiplierPerMinute,
                           EffectType effectType, float tickInterval, float duration)
    {
        _playerStat          = playerStat;
        _damageMultiplierPerMinute = damageMultiplierPerMinute;
        _tickInterval        = Mathf.Max(0.1f, tickInterval);
        _duration            = duration;

        // DamageInfo는 한 번 생성 후 damageAmount만 매 틱 갱신
        _damageInfo = new DamageInfo(0f, effectType, playerStat, gameObject);

        StartCoroutine(TerminateCoroutine());
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer < _tickInterval) return;

        _tickTimer = 0f;
        ApplyDamageToEnemies();
    }

    private void ApplyDamageToEnemies()
    {
        if (_enemiesInZone.Count == 0) return;

        // 분당 데미지 → 틱당 데미지
        float damagePerTick = _playerStat.AttackPower * _damageMultiplierPerMinute / 60f * _tickInterval;
        _damageInfo.damageAmount = damagePerTick;
        _damageInfo.attackSource = gameObject;

        var toRemove = new List<IDamageable>();
        foreach (var enemy in _enemiesInZone)
        {
            if (enemy.IsDead())
            {
                toRemove.Add(enemy);
                continue;
            }
            enemy.TakeDamage(_damageInfo);
        }

        foreach (var dead in toRemove)
            _enemiesInZone.Remove(dead);
    }

    private void OnTriggerEnter(Collider other)
    {
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && !damageable.IsDead())
            _enemiesInZone.Add(damageable);
    }

    private void OnTriggerExit(Collider other)
    {
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            _enemiesInZone.Remove(damageable);
    }

    private IEnumerator TerminateCoroutine()
    {
        yield return new WaitForSeconds(_duration);
        _enemiesInZone.Clear();
        Destroy(gameObject);
    }

    // 맵 클리어/재시작 시에는 즉시 제거 (풀 미등록 오브젝트이므로 Destroy)
    protected override void OnAllStagesCleared()
    {
        StopAllCoroutines();
        _enemiesInZone.Clear();
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var col = GetComponent<SphereCollider>();
        if (col == null) return;
        Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position + col.center, col.radius);
        Gizmos.color = new Color(0.8f, 0.4f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position + col.center, col.radius);
    }
#endif
}
