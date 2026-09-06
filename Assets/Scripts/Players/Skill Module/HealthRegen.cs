using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 체력 재생 스킬. 일정 주기마다 최대 체력의 일정 비율만큼 회복한다.
    /// 레벨이 오를수록 회복량이 늘어난다.
    /// </summary>
    public class HealthRegen : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(0.5f)] private float regenInterval = 3f;
        [SerializeField, Min(0f)] private float maxHpPercentPerLevel = 0.02f;

        private float _timer;
        private PlayerController _player;

        /// <summary> 평균 초당 회복량 (UI 표시용, 실제 회복은 regenInterval마다 목돈으로 발생). </summary>
        public float RegenPerSecond
        {
            get
            {
                if (_player == null || _player.Stat == null || regenInterval <= 0f) return 0f;
                float amount = Mathf.Max(1, Mathf.RoundToInt(_player.Stat.MaxHP * maxHpPercentPerLevel * Level));
                return amount / regenInterval;
            }
        }

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public void Apply()
        {
            _player = PlayerController.Instance;
            _timer = 0f;
        }

        private void Update()
        {
            if (_player == null || _player.IsPlayerDead) return;

            _timer += Time.deltaTime;
            if (_timer < regenInterval) return;

            _timer = 0f;

            int amount = Mathf.Max(1, Mathf.RoundToInt(_player.Stat.MaxHP * maxHpPercentPerLevel * Level));
            _player.Hurt.TryTakeHeal(amount);
        }
    }
}
