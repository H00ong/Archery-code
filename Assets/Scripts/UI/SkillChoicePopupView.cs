using System.Collections.Generic;
using UnityEngine;

public class SkillChoicePopupView : MonoBehaviour
{
    [SerializeField] UI_Skill _uiSkillPrefab;

    public List<UI_Skill> BuildCards(IReadOnlyList<SkillDefinition> defs)
    {
        var list = new List<UI_Skill>(defs.Count);
        foreach (var _ in defs)
            list.Add(Instantiate(_uiSkillPrefab, transform));
        return list;
    }

    private void RemoveAllSkillCards()
    {
        foreach (Transform c in transform) 
            Destroy(c.gameObject);
    }

    public void Open() => gameObject.SetActive(true);
    public void Close() 
    {
        RemoveAllSkillCards();
        gameObject.SetActive(false);
    }
}
