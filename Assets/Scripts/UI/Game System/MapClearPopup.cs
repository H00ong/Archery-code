using UnityEngine;

public class MapClearPopupView : MonoBehaviour
{
    private UI_MapClearView _view;

    public UI_MapClearView GetMapClearView()
    {
        if (_view == null)
            _view = GetComponentInChildren<UI_MapClearView>();

        return _view;
    }

    public void Open()
    {
        _view.gameObject.SetActive(true);
    }

    public void Close()
    {
        _view.gameObject.SetActive(false);
    }
}
