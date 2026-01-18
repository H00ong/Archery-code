using System.Collections.Generic;
using Players.SkillModule;
using UnityEngine;

namespace Players
{
    public class PlayerSkill : MonoBehaviour
    {
        [Header("Skills")]
        [SerializeField] private List<SkillDefinition> skills = new();
        public List<SkillDefinition> availableSkills => skills;

        public readonly Dictionary<PlayerSkillId, PlayerSkillModuleBase> acquiredSkillModule = new();

        public void Init()
        {
            
        }

        public void AcquireSkill(SkillDefinition so)
        {
            if (!availableSkills.Contains(so)) return;

            if (acquiredSkillModule.ContainsKey(so.id))
                UpgradeSkillModule(so);
            else
                LearnSkill(so);
        }

        private void LearnSkill(SkillDefinition so)
        {
            var skill = skills.Find(skill => skill.id == so.id);
            skill.InstallModule(gameObject, this);
            acquiredSkillModule[so.id] = skill.GetModule();
        }

        private void UpgradeSkillModule(SkillDefinition so)
        {
            acquiredSkillModule[so.id].UpdateSkill();

            if (acquiredSkillModule[so.id].Level >= so.maxLevel)
                availableSkills.Remove(so);
        }

        public int GetLevel(SkillDefinition def)
        {
            return acquiredSkillModule.TryGetValue(def.id, out var mod) ? mod.Level : 0;
        }

        public List<SkillDefinition> GetRandomChoices(int count = 3)
        {
            var rng = new System.Random();
            var pool = new List<SkillDefinition>(availableSkills);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            return pool.Count <= count ? pool : pool.GetRange(0, count);
        }


        #region Skill methods

        public void UpgradeAttackSpeed(float modifier) => PlayerController.Instance.Attack.UpdateAttackSpeed(modifier);
        public void UpgradeMoveSpeed(float modifier) => PlayerController.Instance.Move.UpdateMoveSpeed(modifier);

        #endregion
    }
}
