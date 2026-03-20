using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_GameOverView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] Button retryButton;
    [SerializeField] Button lobbyButton;

    void Awake()
    {
        retryButton.GetComponentInChildren<TextMeshProUGUI>().text = "Retry";
        lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Lobby";
    }

    public void Init(UnityAction onRetry, UnityAction onLobby)
    {
        retryButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.RemoveAllListeners();

        retryButton.onClick.AddListener(onRetry);
        lobbyButton.onClick.AddListener(onLobby);
    }
}
