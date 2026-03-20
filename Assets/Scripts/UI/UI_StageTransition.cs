using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UI_StageTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI stageText;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float stageTextHoldDuration = 0.5f;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            if (stageText) stageText.gameObject.SetActive(false);
        }

        /// <summary> 화면을 어둡게 (alpha 0 → 1) </summary>
        public async Awaitable FadeOutAsync()
        {
            canvasGroup.blocksRaycasts = true;
            if (stageText) stageText.gameObject.SetActive(false);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                await Awaitable.NextFrameAsync();
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary> 화면을 밝게 (alpha 1 → 0). stageLabel이 있으면 잠시 표시 후 페이드 </summary>
        public async Awaitable FadeInAsync(string stageLabel = null)
        {
            if (!string.IsNullOrEmpty(stageLabel) && stageText)
            {
                stageText.text = stageLabel;
                stageText.gameObject.SetActive(true);
                await Awaitable.WaitForSecondsAsync(stageTextHoldDuration);
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                await Awaitable.NextFrameAsync();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            if (stageText) stageText.gameObject.SetActive(false);
        }
    }
}
