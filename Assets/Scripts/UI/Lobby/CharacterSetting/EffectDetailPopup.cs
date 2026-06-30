using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EffectDetailPopup : MonoBehaviour
    {
        /// <summary>열린 인스턴스 수. UIManager가 ESC 우선순위 체인에서 가장 나중에 열린 것부터 닫는 데 실용한다.</summary>
        public static int OpenCount { get; private set; }

        // 열린 인스턴스를 스택식으로 추적. 마지막에 열린 것이 인덱스 높은 쪽.
        private static readonly List<EffectDetailPopup> _openInstances = new();

        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;

        /// <summary>ESC 키 등으로 가장 나중에 열린 인스턴스를 달a다. UIManager에서만 호출한다.</summary>
        public static bool CloseTopmost()
        {
            if (_openInstances.Count == 0) return false;
            _openInstances[_openInstances.Count - 1].Close();
            return true;
        }

        public void Init()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);

            gameObject.SetActive(false);
        }

        public void Show(string text)
        {
            detailText.text = text;
            gameObject.SetActive(true);

            // 텍스트 업데이트 후 스크롤 맨 위로 리셋
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            OpenCount++;
            _openInstances.Add(this);
        }

        private void OnDisable()
        {
            OpenCount = Mathf.Max(0, OpenCount - 1);
            _openInstances.Remove(this);
        }
        // ESC 처리는 UIManager.Update()에서 일원화 담당한다. 자체 Update()는 없음.
    }
}
