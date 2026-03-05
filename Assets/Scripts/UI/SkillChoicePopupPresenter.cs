using System.Collections.Generic;
using Players;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 스킬 선택 팬업의 Presenter.
    /// 생성자에서 View만 받고, Show()에서 게임 데이터(PlayerSkill)를 받는다.
    /// </summary>
    public class SkillChoicePopupPresenter
    {
        private const int ChoiceCount = 3;
        private readonly SkillChoicePopupView _view;
        private readonly List<SkillPresenter> _cardPresenters = new();

        // Show() 호출 시에만 필요하므로 일시 보관
        private PlayerSkill _currentPlayerSkill;

        public SkillChoicePopupPresenter(SkillChoicePopupView view)
        {
            _view = view;
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
            var cards = _view.BuildCards(actualCount);

            for (int i = 0; i < actualCount; i++)
                _cardPresenters.Add(new SkillPresenter(cards[i], choices[i], playerSkill, OnChosen));

            _view.Open();
        }

        private void OnChosen(SkillDefinition def)
        {
            _currentPlayerSkill.AcquireSkill(def);
            _view.Close();
            EventBus.Publish(EventType.SkillChosen);
        }
    }
}

