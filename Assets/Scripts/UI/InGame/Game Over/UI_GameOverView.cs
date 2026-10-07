using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_GameOverView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] Button retryButton;
    [SerializeField] Button lobbyButton;

    public void Init(UnityAction onRetry, UnityAction onLobby)
    {
        retryButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.RemoveAllListeners();

        retryButton.onClick.AddListener(onRetry);
        lobbyButton.onClick.AddListener(onLobby);
    }

    public void SetGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"+{gold} G";
    }
}
