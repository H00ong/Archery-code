using UnityEngine;
using System.Collections.Generic;
using Players;

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

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.SkillChosen, OnSkillChosen);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.SkillChosen, OnSkillChosen);
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
            // 다음 프레임에 CheckLevelUp을 실행하여
            // 다른 SkillChosen 핸들러들이 먼저 완료되도록 보장
            StartCoroutine(CheckLevelUpNextFrame());
        }

        private System.Collections.IEnumerator CheckLevelUpNextFrame()
        {
            yield return null; // 다음 프레임까지 대기
            CheckLevelUp();
        }

        private void LevelUp()
        {
            _isLevelingUp = true;
            currentExp -= RequiredExp;

            currentLevel++;

            Debug.Log($"Level Up! Current Level: {currentLevel}");

            EventBus.Publish(EventType.LevelUp);
            ShowSkillChoicePopup();
        }

        // Debug용
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                AddExp(RequiredExp);
            }
        }

        private void ShowSkillChoicePopup()
        {
            UIManager.Instance.ShowSkillChoicePopup(PlayerController.Instance.Skill);
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
    }
}

