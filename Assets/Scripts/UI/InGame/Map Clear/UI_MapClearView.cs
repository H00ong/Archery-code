using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_MapClearView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] Button lobbyButton;

    void Awake()
    {
        var label = lobbyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = "Lobby";
    }

    public void Init(UnityAction onLobby)
    {
        lobbyButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.AddListener(onLobby);
    }

    public void SetGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"+{gold} G";
    }
}
