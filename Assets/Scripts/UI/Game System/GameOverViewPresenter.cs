using UnityEngine;

public class GameOverViewPresenter
{
    public GameOverViewPresenter(UI_GameOverView view, UnityEngine.Events.UnityAction onRetry, UnityEngine.Events.UnityAction onLobby)
    {
        view.Init(onRetry, onLobby);
    }
}
