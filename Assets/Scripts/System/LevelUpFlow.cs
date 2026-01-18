using Players;
using UI;
using UnityEngine;

namespace System
{
    public class LevelUpFlow : MonoBehaviour
    {
        [SerializeField] private SkillChoicePopup skillChoicePopup;

        private SkillChoicePopupPresenter _presenter;

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.LevelUp, ShowSkillChoicePopup);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LevelUp, ShowSkillChoicePopup);
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                EventBus.Publish(EventType.LevelUp);
        }
    }
}