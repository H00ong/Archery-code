using UnityEngine;

public class PauseMenuPopup : MonoBehaviour
{
    [Tooltip("초기화 확인 팝업. 일시정지 메뉴 프리팹 하위에 두고 연결한다.")]
    [SerializeField] private ConfirmPopup confirmPopup;

    private UI_PauseMenuView _view;

    public UI_PauseMenuView GetPauseMenuView()
    {
        if (_view == null)
            _view = GetComponentInChildren<UI_PauseMenuView>(true);

        return _view;
    }

    public ConfirmPopup GetConfirmPopup() => confirmPopup;

    public void Open()
    {
        GetPauseMenuView().gameObject.SetActive(true);
    }

    public void Close()
    {
        GetPauseMenuView().gameObject.SetActive(false);
    }
}
