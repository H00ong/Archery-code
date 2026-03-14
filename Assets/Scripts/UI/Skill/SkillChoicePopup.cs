using System.Collections.Generic;
using UI.UI_Objects;
using UnityEngine;

namespace UI
{
    public class SkillChoicePopup : MonoBehaviour
    {
        private List<UI_SkillView> _cards = new();

        public List<UI_SkillView> BuildCards(int count)
        {
            if (_cards.Count == 0)
                foreach (Transform c in transform)
                    _cards.Add(c.GetComponent<UI_SkillView>());

            return _cards.GetRange(0, count);
        }

        public void Open()
        {
            SetCardsActive(true);
        }

        public void Close()
        {
            SetCardsActive(false);
        }

        void OnDisable()
        {
            _cards.Clear();
        }

        private void SetCardsActive(bool active)
        {
            foreach (var c in _cards)
                c.gameObject.SetActive(active);
        }
    }
}
