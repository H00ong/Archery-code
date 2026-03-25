using Managers;
using UnityEngine;

public class MapClearPopupPresenter
{
    private readonly MapClearPopupView _mapClearPopup;
    private MapClearViewPresenter _viewPresenter;

    public MapClearPopupPresenter(MapClearPopupView view)
    {
        _mapClearPopup = view;
    }

    public void Show()
    {
        var _view = _mapClearPopup.GetMapClearView();

        if (_viewPresenter == null)
            _viewPresenter = new MapClearViewPresenter(_view, OnLobby);

        _mapClearPopup.Open();
    }

    private void OnLobby()
    {
        _mapClearPopup.Close();
        EventBus.Publish(EventType.TransitionToLobby);
    }
}
