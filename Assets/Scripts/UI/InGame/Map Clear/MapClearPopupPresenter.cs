using Managers;
using UnityEngine;

public class MapClearPopupPresenter
{
    private readonly MapClearPopupView _popup;
    private MapClearViewPresenter _viewPresenter;

    public MapClearPopupPresenter(MapClearPopupView view)
    {
        _popup = view;
    }

    public void Show()
    {
        var _view = _popup.GetMapClearView();

        if (_viewPresenter == null)
            _viewPresenter = new MapClearViewPresenter(_view, OnLobby);

        _popup.Open();
    }

    private void OnLobby()
    {
        _popup.Close();
        MapManager.Instance.UpdateMapClearData();
        EventBus.Publish(EventType.TransitionToLobby);
    }
}
