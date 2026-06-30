#if UNITY_EDITOR
using UnityEditor;
#endif
using SaveSystem;
using UnityEngine;

/// <summary>
/// ESC 일시정지 메뉴. 로비/인게임 어디서든 ESC 로 토글되며, 열려 있는 동안 Time.timeScale = 0 으로 멈춘다.
/// 볼륨 슬라이더는 SoundManager 와 연동되며, 값 변경 시 SettingsChanged 이벤트로 자동 저장된다.
/// 영구(DontDestroyOnLoad) 캔버스 아래에 배치되어 씬이 바뀌어도 유지된다.
/// </summary>
public class PauseMenuPresenter
{
    private readonly PauseMenuPopup _popup;
    private bool _viewBound;

    public bool IsOpen { get; private set; }

    public PauseMenuPresenter(PauseMenuPopup popup)
    {
        _popup = popup;
        _popup.Close();
    }

    public void Show()
    {
        if (IsOpen) return;

        var view = _popup.GetPauseMenuView();
        if (!_viewBound)
        {
            float bgm = SoundManager.Instance != null ? SoundManager.Instance.BgmVolume : 1f;
            float sfx = SoundManager.Instance != null ? SoundManager.Instance.SfxVolume : 1f;
            view.Init(OnResume, OnLobby, OnReset, OnQuit, OnBgmChanged, OnSfxChanged, bgm, sfx);
            _viewBound = true;
        }

        _popup.Open();
        IsOpen = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (!IsOpen) return;

        SetPauseInteractable(true);
        _popup.GetConfirmPopup()?.Hide();
        _popup.Close();
        IsOpen = false;
        Time.timeScale = 1f;
    }

    /// <summary>속 일시정지 메뉴 위에 초기화 확인 팭업이 떠 있는지 여부.</summary>
    public bool IsConfirmOpen
    {
        get
        {
            var confirm = _popup.GetConfirmPopup();
            return confirm != null && confirm.IsOpen;
        }
    }

    /// <summary>확인 팭업을 취소(닫기) 동작으로 처리한다. ESC 라우팅용.</summary>
    public void CancelConfirm()
    {
        _popup.GetConfirmPopup()?.Cancel();
    }

    private void OnResume() => Hide();

    private void OnLobby()
    {
        Hide();
        EventBus.Publish(EventType.TransitionToLobby);
    }

    private void OnReset()
    {
        var confirm = _popup.GetConfirmPopup();
        if (confirm == null)
        {
            Debug.LogWarning("[PauseMenu] ConfirmPopup이 연결되지 않아 초기화를 진행할 수 없습니다.");
            return;
        }

        // 확인 팭업이 떠 있는 동안에는 뒤의 일시정지 메뉴 버튼을 누르지 못하게 막는다.
        SetPauseInteractable(false);
        confirm.Show(
            "All progress will be lost.\nAre you sure?",
            OnResetConfirmed, OnResetCancelled);
    }

    private void OnResetConfirmed()
    {
        SetPauseInteractable(true);
        Time.timeScale = 1f;
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetAndRestart();
    }

    private void OnResetCancelled()
    {
        // 취소 시 다시 일시정지 메뉴를 조작 가능하게 되돌린다.
        SetPauseInteractable(true);
    }

    /// <summary>일시정지 메뉴 패널의 버튼 조작 가능 여부를 토글한다. 뒤로의 클릭은 항상 차단된다.</summary>
    private void SetPauseInteractable(bool interactable)
    {
        var view = _popup.GetPauseMenuView();
        if (view == null) return;

        var cg = view.GetComponent<CanvasGroup>();
        if (cg == null) cg = view.gameObject.AddComponent<CanvasGroup>();
        cg.interactable = interactable;
        cg.blocksRaycasts = true;
    }

    private void OnQuit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnBgmChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBgmVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSfxVolume(value);
    }
}
