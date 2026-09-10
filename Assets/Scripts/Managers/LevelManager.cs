using UnityEngine;
using System.Collections.Generic;
using Players;
using System.Threading.Tasks;

namespace Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Status")]
        public int currentLevel = 1;
        public int maxLevel = 10;
        public int currentExp = 0;
        
        public int RequiredExp => GetRequiredExpForLevel(currentLevel);

        [Header("Config")]
        [SerializeField]
        private List<int> expTable = new List<int>
        {
            100, 150, 220, 340, 500, 750, 1100, 1600, 2300
        };

        // 레벨업까지 필요한 처치 수. 마지막 값 이후로는 계속 고정된다.
        private static readonly int[] KillsPerLevelTable = { 3, 5, 7, 11, 13 };

        /// <summary> 현재 레벨에서 레벨업까지 필요한 처치 수 </summary>
        public int KillsToLevelUp => GetKillsPerLevelUp(currentLevel);

        /// <summary> 현재 레벨 기준, 적 1마리가 주는 기준 경험치 (랜덤 보정 전) </summary>
        public int ExpPerKill => GetExpPerKill(currentLevel);

        private bool _isLevelingUp = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ResetLevelData()
        {
            currentLevel = 1;
            currentExp = 0;
            _isLevelingUp = false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.SkillChosen, OnSkillChosen, 100);
            EventBus.Subscribe(EventType.Retry, ResetLevelData);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.SkillChosen, OnSkillChosen);
            EventBus.Unsubscribe(EventType.Retry, ResetLevelData);
        }

        public void AddExp(int amount)
        {
            if (currentLevel >= maxLevel) return;

            currentExp += amount;

            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            if (_isLevelingUp) return;
            if (currentLevel >= maxLevel) return;

            if (currentExp >= RequiredExp)
            {
                LevelUp();
            }

            if (currentLevel >= maxLevel)
            {
                currentExp = 0;
            }
        }

        private void OnSkillChosen()
        {
            _isLevelingUp = false;
            CheckLevelUp();
        }

        private void LevelUp()
        {
            _isLevelingUp = true;
            currentExp -= RequiredExp;

            currentLevel++;

            Debug.Log($"Level Up! Current Level: {currentLevel}");

            EventBus.Publish(EventType.LevelUp);
        }

        private int GetRequiredExpForLevel(int level)
        {
            int index = level - 1;

            if (index < 0 || index >= expTable.Count)
            {
                return 999999;
            }

            return expTable[index];
        }

        public static int GetKillsPerLevelUp(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, KillsPerLevelTable.Length - 1);
            return KillsPerLevelTable[index];
        }

        public int GetExpPerKill(int level)
        {
            if (level >= maxLevel) return 0;

            int required = GetRequiredExpForLevel(level);
            int kills = Mathf.Max(1, GetKillsPerLevelUp(level));

            return Mathf.Max(1, Mathf.RoundToInt(required / (float)kills));
        }
    }
}

