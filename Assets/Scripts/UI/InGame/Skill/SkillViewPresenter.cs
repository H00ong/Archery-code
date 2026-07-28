

using Players;
using UI.UI_Objects;

namespace UI
{
    public class SkillViewPresenter
    {
        public SkillViewPresenter(UI_SkillView view, SkillDefinition def,
                              PlayerSkill playerSkill, UnityEngine.Events.UnityAction<SkillDefinition> onChosen)
        {
            int level = playerSkill.GetLevel(def);
            int nextLevel = level + 1;

            view.SetName(def.id.ToString());

            if (level == 0)
                view.SetLevelNew();
            else
                view.SetLevel(nextLevel, def.maxLevel, willBeMax: nextLevel == def.maxLevel);

            view.SetDescription("Description");
            view.SetClickedAction(() => onChosen?.Invoke(def));
        }
    }
}
