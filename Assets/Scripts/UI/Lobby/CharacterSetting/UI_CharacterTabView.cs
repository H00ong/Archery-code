using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum CharacterActionButtonState
    {
        LevelUp,
        Equip,
        Purchase,
    }

    public class UI_CharacterTabView : MonoBehaviour
    {
        [Header("3D 미리보기")]
        [SerializeField] private RawImage characterDisplay;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Header("액션 버튼")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;

        [Header("우측 캐릭터 정보")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private TextMeshProUGUI nextLevelStatsText;

        [Header("하단 Contents - 현재 착용 캐릭터")]
        [SerializeField] private Image equippedCharacterIcon;
        [SerializeField] private TextMeshProUGUI equippedCharacterNameText;
        [SerializeField] private TextMeshProUGUI equippedLevelText;
        [SerializeField] private TextMeshProUGUI equippedStatsText;

        [Header("골드")]
        [SerializeField] private TextMeshProUGUI goldText;

        public void Init(Action onLeft, Action onRight, Action onAction)
        {
            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();
            actionButton.onClick.RemoveAllListeners();

            leftButton.onClick.AddListener(() => onLeft?.Invoke());
            rightButton.onClick.AddListener(() => onRight?.Invoke());
            actionButton.onClick.AddListener(() => onAction?.Invoke());
        }

        public void SetRenderTexture(RenderTexture rt)
            => characterDisplay.texture = rt;

        public void SetCharacterName(string name)
            => characterNameText.text = name;

        public void SetLockIconActive(bool active)
            => lockIcon.SetActive(active);

        // ── 액션 버튼 ──

        public void SetActionButtonState(CharacterActionButtonState state, int goldCost = -1)
        {
            actionButton.interactable = true;

            switch (state)
            {
                case CharacterActionButtonState.LevelUp:
                    actionButtonText.text = goldCost >= 0
                        ? $"Level Up\n<size=70%>{goldCost} G</size>"
                        : "MAX";
                    actionButton.interactable = goldCost >= 0;
                    break;
                case CharacterActionButtonState.Equip:
                    actionButtonText.text = "Equip";
                    break;
                case CharacterActionButtonState.Purchase:
                    actionButtonText.text = $"Purchase\n<size=70%>{goldCost} G</size>";
                    break;
            }
        }

        public void SetActionButtonInteractable(bool interactable)
            => actionButton.interactable = interactable;

        // ── 우측 스탯 패널 ──

        public void SetLevelText(string text)
            => levelText.text = text;

        public void SetCurrentStatsText(string text)
            => currentStatsText.text = text;

        public void SetNextLevelStatsText(string text)
        {
            nextLevelStatsText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            nextLevelStatsText.text = text;
        }

        public void SetEquippedCharacterIcon(Sprite icon)
        {
            if (equippedCharacterIcon != null && icon != null)
                equippedCharacterIcon.sprite = icon;
        }

        public void SetEquippedCharacterName(string name)
            => equippedCharacterNameText.text = name;

        public void SetEquippedLevelText(string text)
            => equippedLevelText.text = text;

        public void SetEquippedStatsText(string text)
            => equippedStatsText.text = text;

        public void SetGoldText(int gold)
            => goldText.text = $"{gold} G";
    }
}
