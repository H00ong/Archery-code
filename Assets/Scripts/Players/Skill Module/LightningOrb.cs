using Game.Player;

namespace Players.SkillModule
{
    /// <summary>
    /// Lightning 오브 생성 스킬. 레벨당 오브 2개씩 늘어난다.
    /// </summary>
    public class LightningOrb : PlayerSkillModuleBase, IOrbGenerator
    {
        private OrbManager _orbManager;
        private readonly EffectType _type = EffectType.Lightning;
        private int OrbCount => 2 * Level;

        public override void Init(PlayerSkill _skill)
        {
            base.Init(_skill);

            _orbManager = OrbManager.Instance != null ? OrbManager.Instance : FindAnyObjectByType<OrbManager>();
            GenerateOrb();
        }

        public override void UpdateSkill()
        {
            base.UpdateSkill();

            GenerateOrb();
        }

        public void GenerateOrb()
        {
            if (_orbManager == null) return;
            _orbManager.GenerateOrbAsync(_type, OrbCount).Forget();
        }
    }
}
