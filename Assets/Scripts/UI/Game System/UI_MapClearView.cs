using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_MapClearView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mapClearText;
    [SerializeField] Button lobbyButton;

    void Awake()
    {
        lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Lobby";
    }

    public void Init(UnityAction onLobby)
    {
        lobbyButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.AddListener(onLobby);
    }
}
