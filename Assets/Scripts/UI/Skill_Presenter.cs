using UnityEngine;

// Presenter for a single skill UI element
public class Skill_Presenter
{
    readonly Skill_View _view;
    readonly SkillDefinition _def;
    readonly System.Action<SkillDefinition> _onChosen;

    public Skill_Presenter(Skill_View view, SkillDefinition def, System.Action<SkillDefinition> onChosen)
    {
        _view = view; 
        _def = def; 
        _onChosen = onChosen;
        
        Bind();
    }

    public void Bind()
    {
        int curLv = PlayerSkill.GetLevel(_def); // 0이면 미보유

        _view.SetName(_def.id.ToString());
        _view.SetLevel(curLv, _def.maxLevel);
        _view.SetDescription("Description");
        _view.SetClickedAction(() => _onChosen?.Invoke(_def));
    }
}
