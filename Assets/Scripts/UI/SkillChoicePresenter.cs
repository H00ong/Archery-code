using System.Collections.Generic;


// Presenter for skill choice
public class SkillChoicePresenter
{
    readonly SkillChoicePopupView _skillChoicePopup;
    readonly PlayerSkill _playerSkill;
    readonly System.Action _onCompleted;
    readonly List<SkillPresenter> _skillChoicePrsenter = new();

    public SkillChoicePresenter(SkillChoicePopupView skillPopup, PlayerSkill playerSkill, System.Action onCompleted)
    {
        _skillChoicePopup = skillPopup; _playerSkill = playerSkill; _onCompleted = onCompleted;
    }

    public void Show()
    {
        var choices = PlayerSkill.GetRandomChoices(3); // availableSkills에서 3개
        var skillUIViews = _skillChoicePopup.BuildCards(choices);

        for (int i = 0; i < choices.Count; i++)
            _skillChoicePrsenter.Add(new SkillPresenter(skillUIViews[i], choices[i], OnChosen));

        _skillChoicePopup.Open();
    }

    private void OnChosen(SkillDefinition def)
    {
        int before = PlayerSkill.GetLevel(def);
        _playerSkill.AcquireSkill(def);     // 기존 도메인 호출 (수정 없음)
        int after = PlayerSkill.GetLevel(def);

        // 성공 시 닫고 흐름 재개
        if (after > before || (before == 0 && after >= 0)) // 프로젝트 정책에 맞게 단순 검증
        {
            _skillChoicePopup.Close();
            _onCompleted?.Invoke();
        }
    }
}

