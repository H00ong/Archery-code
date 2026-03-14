using UnityEngine;

public class GameOverPopupPresenter
{
    private readonly GameOverPopup _popup;
    private GameOverViewPresenter _viewPresenter;

    public GameOverPopupPresenter(GameOverPopup view)
    {
        _popup = view;
    }

    public void Show()
    {
        var _view = _popup.GetGameOverView();

        if (_viewPresenter == null)
            _viewPresenter = new GameOverViewPresenter(_view, OnRetry, OnLobby);

        _popup.Open();
    }

    private void OnRetry()
    {
        _popup.Close();
        EventBus.Publish(EventType.Retry);
    }
    
    private void OnLobby()
    {
        _popup.Close();
        EventBus.Publish(EventType.TransitionToLobby);
    }
}
