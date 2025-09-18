using UnityEngine;

// Presenter for a single skill UI element
public class SkillPresenter
{
    readonly UI_Skill _view;
    readonly SkillDefinition _def;
    readonly System.Action<SkillDefinition> _onChosen;

    public SkillPresenter(UI_Skill view, SkillDefinition def, System.Action<SkillDefinition> onChosen)
    {
        _view = view; _def = def; _onChosen = onChosen;
        _view.Clicked += () => _onChosen?.Invoke(_def);
        Bind();
    }

    public void Bind()
    {
        int curLv = PlayerSkill.GetLevel(_def); // 0이면 미보유
        _view.SetName(_def.id.ToString());
        _view.SetLevel(curLv, _def.maxLevel);
        _view.SetDescription("Description");
    }
}
