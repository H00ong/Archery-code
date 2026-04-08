using System;
using Managers;

namespace UI
{
    /// <summary>
    /// 설정 팝업 Presenter.
    /// 탭 전환 로직을 관리하고, 각 탭의 Presenter에 위임한다.
    /// </summary>
    public class SettingPopupPresenter
    {
        private readonly SettingPopup _popup;
        private readonly LobbyCharacterCamera _characterCamera;
        private readonly LobbyCharacterManager _lobbyCharacterManager;
        private readonly CharacterTabPresenter _characterTabPresenter;
        private int _currentTab;

        public event Action<bool> OnPopupToggled;

        public SettingPopupPresenter(SettingPopup popup,
            LobbyCharacterCamera characterCamera)
        {
            _popup = popup;
            _characterCamera = characterCamera;
            _lobbyCharacterManager = LobbyCharacterManager.Instance;

            var charTabView = _popup.GetCharacterTabView();
            _characterTabPresenter = new CharacterTabPresenter(
                charTabView,
                _lobbyCharacterManager,
                characterCamera);

            _popup.Init(OnTabSelected, Hide);
            _popup.Close();
        }

        public void Show()
        {
            // 팝업이 열리면 탭과 무관하게 카메라/더미 항상 활성화
            _characterCamera.GetOrCreateRenderTexture();
            _characterCamera.SetupPosition(_lobbyCharacterManager.DummyPosition);
            _characterCamera.EnableCamera();
            
            _lobbyCharacterManager.Show();

            _currentTab = 0;
            _popup.SwitchTab(_currentTab);
            _popup.Open();
            _characterTabPresenter.Activate();
            OnPopupToggled?.Invoke(true);
        }

        public void Hide()
        {
            // 팝업이 닫히면 탭과 무관하게 카메라/더미 비활성화
            _characterCamera.DisableCamera();
            _lobbyCharacterManager.Hide();

            _popup.Close();
            OnPopupToggled?.Invoke(false);
        }

        private void OnTabSelected(int index)
        {
            if (index == _currentTab) return;

            if (_currentTab == 0)
                _characterTabPresenter.Deactivate();

            _currentTab = index;
            _popup.SwitchTab(index);

            if (_currentTab == 0)
                _characterTabPresenter.Activate();
        }
    }
}
