using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 캐릭터 설정 팝업 View.
    /// 탭(캐릭터, 장비, 룬)을 전환하고 팝업 열기/닫기를 담당한다.
    /// </summary>
    public class SettingPopup : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] tabContents;

        private Action _onClose;

        public UI_CharacterTabView GetCharacterTabView()
            => tabContents[0].GetComponent<UI_CharacterTabView>();

        public void Init(Action<int> onTabSelected, Action onClose)
        {
            _onClose = onClose;

            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i].onClick.RemoveAllListeners();
                tabButtons[i].onClick.AddListener(() => onTabSelected?.Invoke(index));
            }
        }

        public void Open() => gameObject.SetActive(true);
        public void Close() => gameObject.SetActive(false);

        public void SwitchTab(int activeIndex)
        {
            for (int i = 0; i < tabContents.Length; i++)
                tabContents[i].SetActive(i == activeIndex);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _onClose?.Invoke();
        }
    }
}
