using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 캐릭터 탭 View.
    /// RenderTexture로 3D 캐릭터를 표시하고, 좌우 전환·선택 버튼과 잠금 아이콘을 관리한다.
    /// </summary>
    public class UI_CharacterTabView : MonoBehaviour
    {
        [SerializeField] private RawImage characterDisplay;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshProUGUI characterNameText;

        public void Init(Action onLeft, Action onRight, Action onSelect)
        {
            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();

            leftButton.onClick.AddListener(() => onLeft?.Invoke());
            rightButton.onClick.AddListener(() => onRight?.Invoke());
            selectButton.onClick.AddListener(() => onSelect?.Invoke());
        }

        public void SetRenderTexture(RenderTexture rt)
            => characterDisplay.texture = rt;

        public void SetCharacterName(string name)
            => characterNameText.text = name;

        public void SetLockIconActive(bool active)
            => lockIcon.SetActive(active);

        public void SetSelectButtonInteractable(bool interactable)
            => selectButton.interactable = interactable;
    }
}
