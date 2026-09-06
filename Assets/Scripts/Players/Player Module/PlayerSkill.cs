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
        public readonly Dictionary<string, PlayerSkillModuleBase> acquiredSkillModule = new();

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

            // maxLevel 1짜리 스킬은 처음 배우는 순간(LearnSkill) 이미 max에 도달하므로,
            // 여기서 한 번만 확인해야 두 경로(Learn/Upgrade) 모두에서 빠짐없이 제거된다.
            if (acquiredSkillModule.TryGetValue(so.id, out var mod) && mod.Level >= so.maxLevel)
                _availableSkills.Remove(so);
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
        }

        public int GetLevel(SkillDefinition def)
        {
            return acquiredSkillModule.TryGetValue(def.id, out var mod) ? mod.Level : 0;
        }

        public List<SkillDefinition> GetRandomChoices(int count = 3)
        {
            var stat = PlayerController.Instance != null ? PlayerController.Instance.Stat : null;
            var pool = _availableSkills.Where(s => s.IsAvailable(stat)).ToList();

            if (pool.Count == 0)
            {
                Debug.LogWarning("[PlayerSkill] 획득 가능한 스킬이 없습니다!");
                return new List<SkillDefinition>();
            }

            var rng = new System.Random();
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
        public void UpgradeAttackPower(int amount) => PlayerController.Instance.Stat.SetBuffAttackPower(amount);
        public void UpgradeProjectileSpeed(float amount) => PlayerController.Instance.Stat.SetBuffProjectileSpeed(amount);

        #endregion
    }
}
