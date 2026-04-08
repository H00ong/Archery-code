using Managers;

namespace UI
{
    /// <summary>
    /// 캐릭터 탭 Presenter.
    /// LobbyCharacterManager와 LobbyCharacterCamera를 조합하여
    /// 캐릭터 전환·선택·잠금 상태를 View에 반영한다.
    /// </summary>
    public class CharacterTabPresenter
    {
        private readonly UI_CharacterTabView _view;
        private readonly LobbyCharacterManager _lobbyCharacterManager;
        private readonly LobbyCharacterCamera _characterCamera;

        public CharacterTabPresenter(UI_CharacterTabView view,
            LobbyCharacterManager lobbyCharacterManager,
            LobbyCharacterCamera characterCamera)
        {
            _view = view;
            _lobbyCharacterManager = lobbyCharacterManager;
            _characterCamera = characterCamera;

            _view.Init(OnLeft, OnRight, OnSelect);
        }

        public void Activate()
        {
            // 카메라/더미 활성화는 SettingPopupPresenter에서 담당
            // 탭 전환 시 View에 RT 재할당 및 표시 갱신만 수행
            var rt = _characterCamera.GetOrCreateRenderTexture();
            _view.SetRenderTexture(rt);
            UpdateView();
        }

        public void Deactivate()
        {
            // 카메라/더미 비활성화는 SettingPopupPresenter에서 담당
        }

        private void OnLeft()
        {
            _lobbyCharacterManager.ChangeCharacter(-1);
            UpdateView();
        }

        private void OnRight()
        {
            _lobbyCharacterManager.ChangeCharacter(1);
            UpdateView();
        }

        private void OnSelect()
        {
            if (_lobbyCharacterManager.IsCurrentCharacterLocked())
                return;

            _lobbyCharacterManager.SelectCurrent();
            UpdateView();
        }

        private void UpdateView()
        {
            string name = _lobbyCharacterManager.GetCurrentCharacterName();
            bool locked = _lobbyCharacterManager.IsCurrentCharacterLocked();

            _view.SetCharacterName(name);
            _view.SetLockIconActive(locked);
            _view.SetSelectButtonInteractable(!locked);
        }
    }
}
