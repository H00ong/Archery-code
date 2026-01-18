using System.Collections;
using System.Collections.Generic;
using Players.SkillModule;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Players
{
    public class PlayerSkill : MonoBehaviour
    {
        [Header("Addressable Settings")]
        [SerializeField] private string skillConfigLabel = "skill_config";

        private Dictionary<PlayerSkillId, SkillDefinition> _skillDict = new();
        private List<SkillDefinition> _availableSkills = new();
        public List<SkillDefinition> availableSkills => _availableSkills;

        public readonly Dictionary<PlayerSkillId, PlayerSkillModuleBase> acquiredSkillModule = new();

        private bool _isSkillLoaded = false;
        private AsyncOperationHandle<IList<SkillDefinition>> _loadHandle;

        public void Init()
        {
            StartCoroutine(LoadAllSkills());
        }

        private void OnDestroy()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }

        private IEnumerator LoadAllSkills()
        {
            _loadHandle = Addressables.LoadAssetsAsync<SkillDefinition>(skillConfigLabel, null);
            yield return _loadHandle;

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _skillDict.Clear();
                _availableSkills.Clear();

                foreach (var skill in _loadHandle.Result)
                {
                    if (skill)
                    {
                        if (!_skillDict.TryAdd(skill.id, skill))
                        {
                            Debug.LogWarning($"[PlayerSkill] Duplicate Skill ID: {skill.id}");
                        }
                        else
                        {
                            _availableSkills.Add(skill);
                        }
                    }
                }
                _isSkillLoaded = true;
                Debug.Log($"[PlayerSkill] Loaded {_skillDict.Count} skills.");
            }
            else
            {
                Debug.LogError("[PlayerSkill] Failed to load skills!");
            }
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
            if (_skillDict.TryGetValue(so.id, out var skill))
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
            if (!_isSkillLoaded || _availableSkills.Count == 0)
            {
                Debug.LogWarning("[PlayerSkill] Skills not loaded yet or no available skills!");
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
        public void UpgradeMoveSpeed(float modifier) => PlayerController.Instance.Move.UpdateMoveSpeed(modifier);

        #endregion
    }
}
