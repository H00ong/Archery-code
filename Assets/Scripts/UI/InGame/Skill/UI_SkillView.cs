using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.UI_Objects
{
    public class UI_SkillView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private TextMeshProUGUI skillLevel;
        [SerializeField] private TextMeshProUGUI skillDescription;
        [SerializeField] private GameObject maxLevelBadge;

        private UnityAction _onClicked;
    
        public void SetName(string s) => skillName.text = s;

        /// <summary>아직 배운 적 없는 스킬 — "Level 1." 표시</summary>
        public void SetLevelNew()
        {
            skillLevel.text = "Level 1.";
            if (maxLevelBadge != null) maxLevelBadge.SetActive(false);
        }

        /// <summary>이미 배운 스킬의 다음 레벨을 표시한다. willBeMax가 true면 MAX 뱃지를 활성화한다.</summary>
        public void SetLevel(int nextLevel, int max, bool willBeMax = false)
        {
            skillLevel.text = $"Lv.{nextLevel}/{max}";
            if (maxLevelBadge != null) maxLevelBadge.SetActive(willBeMax);
        }

        public void SetDescription(string s) => skillDescription.text = s;
        public void SetClickedAction(UnityAction clickedAction) => _onClicked = clickedAction;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _onClicked?.Invoke();
        }
    }
}