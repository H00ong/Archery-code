using UnityEngine;
using System.Collections.Generic;
using Players;
using UI;

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

        [Header("UI")]
        [SerializeField] private SkillChoicePopup skillChoicePopup;

        private bool _isLevelingUp = false;
        private SkillChoicePopupPresenter _presenter;

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
            CheckLevelUp();
        }

        private void LevelUp()
        {
            _isLevelingUp = true;
            currentExp -= RequiredExp;
            
            currentLevel++;

            Debug.Log($"Level Up! Current Level: {currentLevel}");

            ShowSkillChoicePopup();
        }

        private void ShowSkillChoicePopup()
        {
            EnsurePresenterCreated();
            _presenter.Show();
        }

        private void EnsurePresenterCreated()
        {
            if (_presenter != null) return;

            _presenter = new SkillChoicePopupPresenter(
                skillChoicePopup: skillChoicePopup,
                playerSkill: PlayerController.Instance.Skill
            );
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

