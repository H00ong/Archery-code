using Players;
using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 장판 스킬: 주기적으로 플레이어 발 밑에 장판(AOE 지역)을 소환한다.
    /// 장판 안에 있는 적은 분당 데미지(DPS)를 지속적으로 받는다.
    ///
    /// Level 1 → 기본 소환 주기로 장판 생성
    /// Level 2 → 소환 주기 40% 단축 (더 자주 장판 깔림)
    /// </summary>
    public class GroundZoneSkill : PlayerSkillModuleBase
    {
        [Header("장판 설정")]
        [Tooltip("장판 프리팹 (GroundZone 컴포넌트 + SphereCollider(Trigger) 필요)")]
        [SerializeField] private GameObject groundZonePrefab;

        [Tooltip("분당 데미지 = AttackPower × 이 값 (예: 120 → 초당 2×AttackPower)")]
        [SerializeField, Min(1f)] private float damageMultiplierPerMinute = 120f;

        [Tooltip("데미지 틱 간격 (초)")]
        [SerializeField, Min(0.1f)] private float tickInterval = 0.5f;

        [Tooltip("장판 한 장의 지속 시간 (초)")]
        [SerializeField, Min(1f)] private float zoneDuration = 6f;

        [Tooltip("장판 소환 주기 (초). Level 2에서 단축됨.")]
        [SerializeField, Min(0.5f)] private float spawnInterval = 4f;

        [Tooltip("장판 속성")]
        [SerializeField] private EffectType elementType = EffectType.Normal;

        private float _spawnTimer;
        private PlayerController _player;

        public override void Init(PlayerSkill skill)
        {
            base.Init(skill);
            _player = PlayerController.Instance;

            // 스킬 획득 즉시 첫 장판 소환하도록 타이머를 꽉 채움
            _spawnTimer = spawnInterval;
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();
            // Level 2: 소환 주기 40% 단축
            spawnInterval = Mathf.Max(0.5f, spawnInterval * 0.6f);
        }

        private void Update()
        {
            if (_player == null || _player.IsPlayerDead) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < spawnInterval) return;

            _spawnTimer = 0f;
            SpawnZone();
        }

        private void SpawnZone()
        {
            if (groundZonePrefab == null)
            {
                Debug.LogWarning("[GroundZoneSkill] groundZonePrefab이 할당되지 않았습니다.");
                return;
            }

            Vector3 spawnPos = _player.transform.position;
            var go = Object.Instantiate(groundZonePrefab, spawnPos, Quaternion.identity);

            if (!go.TryGetComponent<GroundZone>(out var zone))
            {
                Debug.LogError("[GroundZoneSkill] groundZonePrefab에 GroundZone 컴포넌트가 없습니다.");
                Object.Destroy(go);
                return;
            }

            zone.Initialize(
                _player.Stat,
                damageMultiplierPerMinute,
                elementType,
                tickInterval,
                zoneDuration
            );
        }
    }
}
