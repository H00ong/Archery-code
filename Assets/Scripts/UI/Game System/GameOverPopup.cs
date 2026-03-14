using UnityEngine;

public class GameOverPopup : MonoBehaviour
{
    private UI_GameOverView _view;
    public UI_GameOverView GetGameOverView()
    {
        if (_view == null)
            _view = GetComponentInChildren<UI_GameOverView>();

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
