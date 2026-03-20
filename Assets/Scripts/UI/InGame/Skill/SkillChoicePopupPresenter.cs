using System.Collections.Generic;
using Players;
using UnityEngine;

namespace UI
{
    public class SkillChoicePopupPresenter
    {
        private const int ChoiceCount = 3;
        private readonly SkillChoicePopup _popup;
        private readonly List<SkillViewPresenter> _cardPresenters = new();
        private PlayerSkill _currentPlayerSkill;

        public SkillChoicePopupPresenter(SkillChoicePopup view)
        {
            _popup = view;
        }

        public void Show(PlayerSkill playerSkill)
        {
            _currentPlayerSkill = playerSkill;
            _cardPresenters.Clear();

            var choices = playerSkill.GetRandomChoices(ChoiceCount);

            // 획득 가능한 스킬이 없으면 팝업 없이 즉시 재개
            if (choices.Count == 0)
            {
                EventBus.Publish(EventType.SkillChosen);
                return;
            }

            // 남은 스킬이 ChoiceCount보다 적을 수 있으므로 실제 개수 기준으로 한정
            int actualCount = Mathf.Min(ChoiceCount, choices.Count);
            var cards = _popup.BuildCards(actualCount);

            for (int i = 0; i < actualCount; i++)
            {
                _cardPresenters.Add(new SkillViewPresenter(cards[i], choices[i], playerSkill, OnChosen));
            }

            _popup.Open();
        }

        private void OnChosen(SkillDefinition def)
        {
            _currentPlayerSkill.AcquireSkill(def);
            _popup.Close();
            EventBus.Publish(EventType.SkillChosen);
        }
    }
}

