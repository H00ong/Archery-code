using UI;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    [Header("Character Setting")]
    [SerializeField] private SettingPopup settingPopup;
    [SerializeField] private LobbyCharacterCamera lobbyCharacterCamera;
    [SerializeField] private Button settingsButton;

    public SettingPopup SettingPopup => settingPopup;
    public LobbyCharacterCamera LobbyCharacterCamera => lobbyCharacterCamera;
    public Button SettingsButton => settingsButton;
}
