using System.Collections.Generic;
using UI.UI_Objects;
using UnityEngine;

namespace UI
{
    public class SkillChoicePopup : MonoBehaviour
    {
        [SerializeField] private SkillView uiSkillPrefab;

        public List<SkillView> BuildCards(int count)
        {
            List<SkillView> list = new List<SkillView>();

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(uiSkillPrefab, transform);
                list.Add(go);
            }

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
}
