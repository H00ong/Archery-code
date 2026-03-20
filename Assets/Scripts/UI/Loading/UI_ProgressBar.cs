using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UI_ProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Animation")]
        [SerializeField] private float lerpSpeed = 5f;

        private float _targetProgress;

        private void Awake()
        {
            slider.value = 0f;
            _targetProgress = 0f;
        }

        private void Update()
        {
            if (Mathf.Approximately(slider.value, _targetProgress)) return;
            slider.value = Mathf.MoveTowards(slider.value, _targetProgress, lerpSpeed * Time.deltaTime);
        }

        /// <summary> 진행률 설정 (0 ~ 1) </summary>
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
        }

        /// <summary> 상태 텍스트 갱신 </summary>
        public void SetStatus(string message)
        {
            if (statusText) statusText.text = message;
        }
    }
}
