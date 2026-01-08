using Players;
using UI;
using UnityEngine;

namespace System
{
    public class LevelUpFlow : MonoBehaviour
    {
        [SerializeField] private SkillChoicePopupView skillChoicePopup;
        [SerializeField] private Transform popupContainer;

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
            var popup = Instantiate(skillChoicePopup, popupContainer);
            var presenter = new SkillChoicePopupPresenter( 
                skillPopup: popup, 
                playerSkill: PlayerController.Instance.Skill
            );

            presenter.Show();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                EventBus.Publish(EventType.LevelUp);
        }
    }
}