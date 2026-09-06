using Managers;

namespace Players.SkillModule
{
    /// <summary>
    /// Poison 배럴 생성 스킬. 레벨당 동시에 존재하는 배럴 수가 1개씩 늘어난다.
    /// </summary>
    public class PoisonBarrel : PlayerSkillModuleBase, IPickupBarrel
    {
        private BarrelManager _barrelManager;
        private readonly EffectType _type = EffectType.Poison;
        private int BarrelCount => Level;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);

            _barrelManager = BarrelManager.Instance != null ? BarrelManager.Instance : FindAnyObjectByType<BarrelManager>();
            GenerateActivate();
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();

            GenerateActivate();
        }

        public void GenerateActivate()
        {
            if (_barrelManager == null) return;
            _barrelManager.UpdateBarrelSkill(_type, BarrelCount);
        }
    }
}
