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
        // popup 자신 및 부모 계층이 InGame 씨 전환 등으로 비활성화되어 있을 수 있으므로
        // Open 시 자신과 부모 루트 Canvas를 명시적으로 활성화해 준다.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        GetPauseMenuView().gameObject.SetActive(true);
    }

    public void Close()
    {
        GetPauseMenuView().gameObject.SetActive(false);
    }
}
