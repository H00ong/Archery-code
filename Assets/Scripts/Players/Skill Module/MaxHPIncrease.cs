using UnityEngine;

namespace Players.SkillModule
{
    /// <summary>
    /// 최대 체력 증가 스킬. 증가한 만큼 현재 체력도 즉시 함께 회복된다.
    /// </summary>
    public class MaxHPIncrease : PlayerSkillModuleBase, IPlayerUpgrader
    {
        [SerializeField, Min(1)] private int hpPerLevel = 20;

        // 이미 반영한 누적량. 재적용 시 증가분만 더하기 위해 보관한다.
        private int _appliedAmount;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);
            Apply();
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();
            Apply();
        }

        public void Apply()
        {
            var player = PlayerController.Instance;
            if (player == null || player.Stat == null) return;

            int target = hpPerLevel * Level;
            int delta = target - _appliedAmount;
            if (delta <= 0) return;

            _appliedAmount = target;
            player.Stat.SetBuffMaxHP(target);
            player.Health?.IncreaseMaxHealth(delta);
        }
    }
}
