using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.UI_Objects
{
    public class UI_SkillView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private TextMeshProUGUI skillLevel;
        [SerializeField] private TextMeshProUGUI skillDescription;
        [SerializeField] private Image skillIcon;
        [SerializeField] private GameObject maxLevelBadge;

        private UnityAction _onClicked;
    
        public void SetName(string s)
        {
            skillName.text = s;
            skillName.enableAutoSizing = true; // 긴 스킬 이름이 박스 밖으로 넘치지 않도록 폰트 크기를 자동 축소
        }
        public void SetIcon(Sprite icon)
        {
            if (skillIcon == null)
            {
                Debug.LogWarning($"[{nameof(UI_SkillView)}] skillIcon is not assigned.", this);
                return;
            }

            skillIcon.sprite = icon;
            skillIcon.color = Color.white;
            skillIcon.preserveAspect = true;
            skillIcon.enabled = icon != null;
        }

        /// <summary>선택지에 뜬 스킬이 배우면 도달할 레벨을 "Level n / max" 형식으로 표시한다. willBeMax가 true면 MAX 뱃지를 활성화한다.</summary>
        public void SetLevel(int nextLevel, int max, bool willBeMax = false)
        {
            skillLevel.text = $"Level {nextLevel} / {max}";
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