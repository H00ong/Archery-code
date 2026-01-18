using System.Collections.Generic;
using Players;


namespace UI
{
    public class SkillChoicePopupPresenter
    {
        private readonly SkillChoicePopup _skillChoicePopup;
        private readonly PlayerSkill _playerSkill;
        private readonly List<SkillPresenter> _skillChoicePresenter = new();

        public SkillChoicePopupPresenter(SkillChoicePopup skillChoicePopup, PlayerSkill playerSkill)
        {
            _skillChoicePopup = skillChoicePopup; 
            _playerSkill = playerSkill; 
        }

        public void Show()
        {
            _skillChoicePresenter.Clear();
            
            int count = 3;

            var choices = _playerSkill.GetRandomChoices(count);
            var skillUiObjects = _skillChoicePopup.BuildCards(count);

            for (int i = 0; i < count; i++)
                _skillChoicePresenter.Add(new SkillPresenter(skillUiObjects[i], choices[i], OnChosen));

            _skillChoicePopup.Open();
        }

        private void OnChosen(SkillDefinition def)
        {
            _playerSkill.AcquireSkill(def);
            _skillChoicePopup.Close();
            EventBus.Publish(EventType.SkillChosen);
        }
    }
}

