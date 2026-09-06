

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
            view.SetIcon(def.icon);

            view.SetLevel(nextLevel, def.maxLevel, willBeMax: nextLevel == def.maxLevel);

            view.SetDescription(string.IsNullOrWhiteSpace(def.description)
                ? "Description not set."
                : def.description);
            view.SetClickedAction(() => onChosen?.Invoke(def));
        }
    }
}
