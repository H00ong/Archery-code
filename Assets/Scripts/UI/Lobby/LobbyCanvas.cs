using UI;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    [Header("Character Setting")]
    [SerializeField] private SettingPopup settingPopup;
    [SerializeField] private LobbyCharacterCamera lobbyCharacterCamera;
    [SerializeField] private Button settingsButton;

    [Header("Enter Game Hint")]
    [SerializeField] private UI_KeyHintPopup enterGameHint;

    public SettingPopup SettingPopup => settingPopup;
    public LobbyCharacterCamera LobbyCharacterCamera => lobbyCharacterCamera;
    public Button SettingsButton => settingsButton;
    public UI_KeyHintPopup EnterGameHint => enterGameHint;

    private void Start()
    {
        if (enterGameHint) enterGameHint.Show(GameKeys.LobbyToInGame);
    }
}
