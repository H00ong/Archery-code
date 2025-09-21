using System.Collections.Generic;


// Presenter for skill choice
public class SkillChoicePopup_Presenter
{
    readonly SkillChoicePopup_View _skillChoicePopup;
    readonly PlayerSkill _playerSkill;
    readonly System.Action _onCompleted;
    readonly List<Skill_Presenter> _skillChoicePresenter = new();

    public SkillChoicePopup_Presenter(SkillChoicePopup_View skillPopup, PlayerSkill playerSkill, System.Action onCompleted)
    {
        _skillChoicePopup = skillPopup; 
        _playerSkill = playerSkill; 
        _onCompleted = onCompleted;
    }

    public void Show()
    {
        var choices = PlayerSkill.GetRandomChoices(3); // availableSkills¿¡¼­ 3°³
        var skillUIViews = _skillChoicePopup.BuildCards(choices);

        for (int i = 0; i < choices.Count; i++)
            _skillChoicePresenter.Add(new Skill_Presenter(skillUIViews[i], choices[i], OnChosen));

        _skillChoicePopup.Open();
    }

    private void OnChosen(SkillDefinition def)
    {
        _playerSkill.AcquireSkill(def);
        _onCompleted?.Invoke();
        _skillChoicePopup.Close();
    }
}

