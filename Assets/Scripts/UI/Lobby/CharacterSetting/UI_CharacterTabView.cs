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
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private TextMeshProUGUI levelGrowthStatText;

        [Header("하단 Contents - 현재 착용 캐릭터")]
        [SerializeField] private Image equippedCharacterIcon;
        [SerializeField] private TextMeshProUGUI equippedCharacterNameText;
        [SerializeField] private TextMeshProUGUI equippedLevelText;
        [SerializeField] private TextMeshProUGUI equippedStatsText;
        

        [Header("골드")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("현재 스탯 상세 팝업")]
        [SerializeField] private Button currentStatsDetailButton;
        [SerializeField] private EffectDetailPopup currentStatsDetailPopup;

        [Header("성장 상세 팝업")]
        [SerializeField] private Button growthDetailButton;
        [SerializeField] private EffectDetailPopup growthDetailPopup;

        [Header("착용 스탯 상세 팝업")]
        [SerializeField] private Button equippedStatsDetailButton;
        [SerializeField] private EffectDetailPopup equippedStatsDetailPopup;

        public void Init(Action onLeft, Action onRight, Action onAction, Action onGrowthDetail, Action onCurrentStatsDetail, Action onEquippedStatsDetail)
        {
            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();
            actionButton.onClick.RemoveAllListeners();

            leftButton.onClick.AddListener(() => onLeft?.Invoke());
            rightButton.onClick.AddListener(() => onRight?.Invoke());
            actionButton.onClick.AddListener(() => onAction?.Invoke());

            if (growthDetailButton != null)
            {
                growthDetailButton.onClick.RemoveAllListeners();
                growthDetailButton.onClick.AddListener(() => onGrowthDetail?.Invoke());
            }

            if (currentStatsDetailButton != null)
            {
                currentStatsDetailButton.onClick.RemoveAllListeners();
                currentStatsDetailButton.onClick.AddListener(() => onCurrentStatsDetail?.Invoke());
            }

            if (equippedStatsDetailButton != null)
            {
                equippedStatsDetailButton.onClick.RemoveAllListeners();
                equippedStatsDetailButton.onClick.AddListener(() => onEquippedStatsDetail?.Invoke());
            }

            if (growthDetailPopup != null)
                growthDetailPopup.Init();

            if (currentStatsDetailPopup != null)
                currentStatsDetailPopup.Init();

            if (equippedStatsDetailPopup != null)
                equippedStatsDetailPopup.Init();
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

        public void SetCurrentStatsText(string text)
            => currentStatsText.text = text;

        public void SetLevelGrowthStatText(string text)
        {
            levelGrowthStatText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            levelGrowthStatText.text = text;
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

        public void ShowGrowthDetailPopup(string detailText)
        {
            if (growthDetailPopup != null)
                growthDetailPopup.Show(detailText);
        }

        public void SetGrowthDetailButtonActive(bool active)
        {
            if (growthDetailButton != null)
                growthDetailButton.gameObject.SetActive(active);
        }

        public void ShowCurrentStatsDetailPopup(string detailText)
        {
            if (currentStatsDetailPopup != null)
                currentStatsDetailPopup.Show(detailText);
        }

        public void SetCurrentStatsDetailButtonActive(bool active)
        {
            if (currentStatsDetailButton != null)
                currentStatsDetailButton.gameObject.SetActive(active);
        }

        public void ShowEquippedStatsDetailPopup(string detailText)
        {
            if (equippedStatsDetailPopup != null)
                equippedStatsDetailPopup.Show(detailText);
        }

        public void SetEquippedStatsDetailButtonActive(bool active)
        {
            if (equippedStatsDetailButton != null)
                equippedStatsDetailButton.gameObject.SetActive(active);
        }

        public void CloseAllDetailPopups()
        {
            growthDetailPopup?.Close();
            currentStatsDetailPopup?.Close();
            equippedStatsDetailPopup?.Close();
        }
    }
}
