using System.Collections.Generic;
using Players;


namespace UI
{
    public class SkillChoicePopupPresenter
    {
        private readonly SkillChoicePopupView _skillChoicePopup;
        private readonly PlayerSkill _playerSkill;
        private readonly List<SkillPresenter> _skillChoicePresenter = new();

        public SkillChoicePopupPresenter(SkillChoicePopupView skillPopup, PlayerSkill playerSkill)
        {
            _skillChoicePopup = skillPopup; 
            _playerSkill = playerSkill; 
        }

        public void Show()
        {
            var choices = _playerSkill.GetRandomChoices(3);
            var skillUiObjects = _skillChoicePopup.BuildCards(choices.Count);

            for (int i = 0; i < choices.Count; i++)
                _skillChoicePresenter.Add(new SkillPresenter(skillUiObjects[i], choices[i], OnChosen));

            _skillChoicePopup.Open();
        }

        private void OnChosen(SkillDefinition def)
        {
            _playerSkill.AcquireSkill(def);
            EventBus.Publish(EventType.SkillChosen);
            _skillChoicePopup.Close();
        }
    }
}

