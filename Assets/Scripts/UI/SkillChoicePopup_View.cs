using System.Collections.Generic;
using UnityEngine;

public class SkillChoicePopup_View : MonoBehaviour
{
    [SerializeField] Skill_View _uiSkillPrefab;

    public List<Skill_View> BuildCards(IReadOnlyList<SkillDefinition> defs)
    {
        var list = new List<Skill_View>(defs.Count);
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
