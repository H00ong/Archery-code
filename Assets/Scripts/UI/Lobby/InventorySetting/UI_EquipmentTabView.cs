using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum EquipmentActionButtonState
    {
        LevelUp,
        Equip,
        Purchase,
    }

    /// <summary>
    /// Inventory 하부 탭(Weapon/Armor/Shoes) 1개에 대응되는 View.
    /// 구조는 UI_CharacterTabView와 동일하되, 3D 미리보기 대신 장비 아이콘(Image)을 사용한다.
    /// </summary>
    public class UI_EquipmentTabView : MonoBehaviour
    {
        [Header("장비 미리보기")]
        [SerializeField] private Image equipmentDisplay;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshProUGUI equipmentNameText;

        [Header("액션 버튼")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;

        [Header("우측 장비 정보")]
        [SerializeField] private TextMeshProUGUI currentStatsText;
        [SerializeField] private TextMeshProUGUI levelGrowthStatText;

        [Header("하단 Contents - 현재 착용 장비")]
        [SerializeField] private Image equippedEquipmentIcon;
        [SerializeField] private TextMeshProUGUI equippedEquipmentNameText;
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

        public void Init(Action onLeft, Action onRight, Action onAction,
            Action onGrowthDetail, Action onCurrentStatsDetail, Action onEquippedStatsDetail)
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

        public void SetEquipmentIcon(Sprite icon)
        {
            if (equipmentDisplay != null)
            {
                equipmentDisplay.sprite = icon;
                equipmentDisplay.enabled = icon != null;
            }
        }

        public void SetEquipmentName(string name)
            => equipmentNameText.text = name;

        public void SetLockIconActive(bool active)
            => lockIcon.SetActive(active);

        // ── 액션 버튼 ──

        public void SetActionButtonState(EquipmentActionButtonState state, int goldCost = -1)
        {
            actionButton.interactable = true;

            switch (state)
            {
                case EquipmentActionButtonState.LevelUp:
                    actionButtonText.text = goldCost >= 0
                        ? $"Level Up\n<size=70%>{goldCost} G</size>"
                        : "MAX";
                    actionButton.interactable = goldCost >= 0;
                    break;
                case EquipmentActionButtonState.Equip:
                    actionButtonText.text = "Equip";
                    break;
                case EquipmentActionButtonState.Purchase:
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

        public void SetEquippedEquipmentIcon(Sprite icon)
        {
            if (equippedEquipmentIcon != null && icon != null)
                equippedEquipmentIcon.sprite = icon;
        }

        public void SetEquippedEquipmentName(string name)
            => equippedEquipmentNameText.text = name;

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
