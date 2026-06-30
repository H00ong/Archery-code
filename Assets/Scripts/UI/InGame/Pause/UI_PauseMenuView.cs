using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_PauseMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button quitButton;

    [Header("Volume")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Optional Labels")]
    [SerializeField] private TextMeshProUGUI titleText;

    public void Init(UnityAction onResume, UnityAction onLobby, UnityAction onReset, UnityAction onQuit,
                     UnityAction<float> onBgmChanged, UnityAction<float> onSfxChanged,
                     float bgmValue, float sfxValue)
    {
        if (titleText != null)
            titleText.text = "Pause";

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(onResume);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.RemoveAllListeners();
            lobbyButton.onClick.AddListener(onLobby);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(onReset);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(onQuit);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.SetValueWithoutNotify(bgmValue);
            bgmSlider.onValueChanged.AddListener(onBgmChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(sfxValue);
            sfxSlider.onValueChanged.AddListener(onSfxChanged);
        }
    }
}
