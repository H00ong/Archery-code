using System;
using Managers;

namespace UI
{
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
            _characterCamera.GetOrCreateRenderTexture();
            _characterCamera.SetupPosition(_lobbyCharacterManager.DummyPosition);
            _characterCamera.SetActive(true);
            
            _lobbyCharacterManager.Show();

            _currentTab = 0;

            _popup.SwitchTab(_currentTab);
            _popup.Open();

            _characterTabPresenter.Activate();

            OnPopupToggled?.Invoke(true);
        }

        public void Hide()
        {
            _characterCamera.SetActive(false);
            _lobbyCharacterManager.Hide();

            _popup.Close();
            OnPopupToggled?.Invoke(false);
        }

        private void OnTabSelected(int index)
        {
            if (index == _currentTab) return;

            _currentTab = index;
            _popup.SwitchTab(index);

            if (_currentTab == 0)
                _characterTabPresenter.Activate();
        }
    }
}