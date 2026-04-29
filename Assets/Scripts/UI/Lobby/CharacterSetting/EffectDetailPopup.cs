using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EffectDetailPopup : MonoBehaviour
    {
        public static int OpenCount { get; private set; }

        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;

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
        }

        private void OnDisable()
        {
            OpenCount = Mathf.Max(0, OpenCount - 1);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
        }
    }
}
