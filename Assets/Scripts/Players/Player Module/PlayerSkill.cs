using System.Collections.Generic;
using System.Linq;
using Managers;
using Players.SkillModule;
using UnityEngine;

namespace Players
{
    public class PlayerSkill : MonoBehaviour
    {
        private List<SkillDefinition> _availableSkills = new();
        public List<SkillDefinition> availableSkills => _availableSkills;
        public readonly Dictionary<PlayerSkillId, PlayerSkillModuleBase> acquiredSkillModule = new();

        public void Init()
        {
            var skillManager = SkillManager.Instance;
            if (skillManager == null || !skillManager.IsLoaded)
            {
                Debug.LogError("[PlayerSkill] SkillManager가 준비되지 않았습니다. InitManager에서 LoadAllSkillsAsync()를 먼저 호출하세요.");
                return;
            }

            _availableSkills = skillManager.AllSkills.ToList();
            acquiredSkillModule.Clear();
            Debug.Log($"[PlayerSkill] {_availableSkills.Count}개 스킬로 초기화 완료.");
        }

        public void AcquireSkill(SkillDefinition so)
        {
            if (!_availableSkills.Contains(so)) return;

            if (acquiredSkillModule.ContainsKey(so.id))
                UpgradeSkillModule(so);
            else
                LearnSkill(so);
        }

        private void LearnSkill(SkillDefinition so)
        {
            if (SkillManager.Instance.SkillDict.TryGetValue(so.id, out var skill))
            {
                skill.InstallModule(gameObject, this);
                acquiredSkillModule[so.id] = skill.GetModule();
            }
        }

        private void UpgradeSkillModule(SkillDefinition so)
        {
            acquiredSkillModule[so.id].UpdateSkill();

            if (acquiredSkillModule[so.id].Level >= so.maxLevel)
                _availableSkills.Remove(so);
        }

        public int GetLevel(SkillDefinition def)
        {
            return acquiredSkillModule.TryGetValue(def.id, out var mod) ? mod.Level : 0;
        }

        public List<SkillDefinition> GetRandomChoices(int count = 3)
        {
            if (_availableSkills.Count == 0)
            {
                Debug.LogWarning("[PlayerSkill] 획득 가능한 스킬이 없습니다!");
                return new List<SkillDefinition>();
            }

            var rng = new System.Random();
            var pool = new List<SkillDefinition>(_availableSkills);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            return pool.Count <= count ? pool : pool.GetRange(0, count);
        }


        #region Skill methods

        public void UpgradeAttackSpeed(float modifier) => PlayerController.Instance.Attack.UpdateAttackSpeed(modifier);
        public void UpgradeMoveSpeed(float modifier) => PlayerController.Instance.Movement.UpdateMoveSpeed(modifier);

        #endregion
    }
}
