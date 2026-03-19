using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    /// <summary>
    /// 전체 SkillDefinition 카탈로그를 Addressable에서 로드하고 보관하는 매니저.
    /// DontDestroyOnLoad 싱글톤. InitManager Phase 2에서 미리 로드된다.
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        [Header("Addressable Settings")]
        [SerializeField] private string skillConfigLabel = "skill_config";

        private readonly Dictionary<string, SkillDefinition> _skillDict = new();
        private readonly List<SkillDefinition> _allSkills = new();
        private AsyncOperationHandle<IList<SkillDefinition>> _loadHandle;

        public bool IsLoaded { get; private set; }

        /// <summary> 로드된 전체 스킬 목록 (읽기 전용) </summary>
        public IReadOnlyList<SkillDefinition> AllSkills => _allSkills;

        /// <summary> ID → SkillDefinition 매핑 (읽기 전용) </summary>
        public IReadOnlyDictionary<string, SkillDefinition> SkillDict => _skillDict;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }

        /// <summary>
        /// Addressable 라벨에서 SkillDefinition 에셋을 비동기 로드한다.
        /// InitManager Phase 2에서 Space 키 입력 이전에 호출된다.
        /// </summary>
        public async Awaitable LoadAllSkillsAsync()
        {
            if (IsLoaded && _allSkills.Count > 0)
            {
                Debug.Log("[SkillManager] 스킬 이미 로드됨, 스킵.");
                return;
            }

            _loadHandle = Addressables.LoadAssetsAsync<SkillDefinition>(skillConfigLabel, null);
            await _loadHandle.Task;
            destroyCancellationToken.ThrowIfCancellationRequested();

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _skillDict.Clear();
                _allSkills.Clear();

                foreach (var skill in _loadHandle.Result)
                {
                    if (skill == null) continue;

                    if (!_skillDict.TryAdd(skill.id, skill))
                        Debug.LogWarning($"[SkillManager] 중복 스킬 ID: {skill.id}");
                    else
                        _allSkills.Add(skill);
                }

                IsLoaded = true;
                Debug.Log($"[SkillManager] {_skillDict.Count}개 스킬 로드 완료.");
            }
            else
            {
                IsLoaded = false;

                if (_loadHandle.IsValid())
                    Addressables.Release(_loadHandle);

                Debug.LogError("[SkillManager] 스킬 로드 실패.");
                throw new System.InvalidOperationException(
                    $"[SkillManager] Addressable 로드 실패 (label: {skillConfigLabel})");
            }
        }
    }
}
