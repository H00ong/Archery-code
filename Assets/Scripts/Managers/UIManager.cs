using Players;
using UI;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Game System")]
        [SerializeField] private GameObject gameClearPopupPrefab;
        [SerializeField] private GameObject gameOverPopupPrefab;

        [Space]
        [Header("Skill")]
        [SerializeField] private GameObject skillChoicePopupPrefab;

        [Space]
        [Header("Pause Menu")]
        [Tooltip("DDL-Canvas 아래에 배치된 PauseMenuPopup 씬 오브젝트. DontDestroyOnLoad 계층 안에 있어서 씬이 바뀌어도 유지된다.")]
        [SerializeField] private PauseMenuPopup pauseMenuPopup;

        [Space]
        [Header("Stage Transition")]
        [SerializeField] private UI_StageTransition stageTransition;

        [Space]
        [Header("InGame HUD")]
        private InGameHud _inGameHud;

        private MapClearPopupPresenter _gameClearPresenter;
        private GameOverPopupPresenter _gameOverPresenter;
        private SkillChoicePopupPresenter _skillChoicePresenter;
        private SettingPopupPresenter _settingPresenter;
        private PauseMenuPresenter _pausePresenter;

        // 인게임 모달(게임오버/클리어/스킬 선택)이 떠 있는 동안에는 일시정지를 열지 않는다.
        private bool _modalOpen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (pauseMenuPopup != null)
                    _pausePresenter = new PauseMenuPresenter(pauseMenuPopup);
                else
                    Debug.LogWarning("[UIManager] pauseMenuPopup이 비어 있어 일시정지 메뉴가 비활성화됩니다. 인스펙터에서 PauseMenuPopup을 연결하세요.");
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnEnable()
        {
            EventBus.Subscribe(EventType.LobbySceneLoaded, SetupSettingPopup);
            EventBus.Subscribe(EventType.InGameSceneLoaded, OnInGameSceneLoaded);
            EventBus.Subscribe(EventType.TransitionToLobby, ClearDataInMap);
            EventBus.Subscribe(EventType.Retry, ClearDataInMap);
            EventBus.Subscribe(EventType.LevelUp, ShowSkillChoicePopup);
            EventBus.Subscribe(EventType.SkillChosen, OnSkillChosen);
            EventBus.Subscribe(EventType.PlayerDied, ShowGameOverPopup);
            EventBus.Subscribe(EventType.MapCleared, ShowGameClearPopup);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LobbySceneLoaded, SetupSettingPopup);
            EventBus.Unsubscribe(EventType.InGameSceneLoaded, OnInGameSceneLoaded);
            EventBus.Unsubscribe(EventType.TransitionToLobby, ClearDataInMap);
            EventBus.Unsubscribe(EventType.Retry, ClearDataInMap);
            EventBus.Unsubscribe(EventType.LevelUp, ShowSkillChoicePopup);
            EventBus.Unsubscribe(EventType.SkillChosen, OnSkillChosen);
            EventBus.Unsubscribe(EventType.PlayerDied, ShowGameOverPopup);
            EventBus.Unsubscribe(EventType.MapCleared, ShowGameClearPopup);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // 로딩 씬에서는 일시정지 메뉴를 열지 않는다. (로비 / 인게임에서만 동작)
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState == SceneState.Loading) return;

            // 0) 초기화 확인 팝업이 떠 있으면 ESC는 일시정지를 닫지 않고 그 팝업의 '취소'로 동작한다.
            if (_pausePresenter != null && _pausePresenter.IsConfirmOpen)
            {
                _pausePresenter.CancelConfirm();
                return;
            }

            // 1) 일시정지가 열려 있으면 최우선으로 닫는다.
            if (_pausePresenter != null && _pausePresenter.IsOpen)
            {
                _pausePresenter.Hide();
                return;
            }

            // 2) 효과 상세 팝업이 열려 있으면 가장 나중에 열린 것부터 닫는다.
            if (UI.EffectDetailPopup.CloseTopmost()) return;

            // 3) 로비 설정 팝업이 열려 있으면 그것부터 닫는다.
            if (_settingPresenter != null && _settingPresenter.IsOpen)
            {
                _settingPresenter.Hide();
                return;
            }

            // 4) 인게임 모달(게임오버/클리어/스킬 선택)이 떠 있으면 일시정지를 열지 않는다.
            if (_modalOpen) return;

            // 5) 일시정지 메뉴 열기.
            _pausePresenter?.Show();
        }

        private void ShowSkillChoicePopup()
        {   
            if (_skillChoicePresenter == null)
            {
                var canvas = FindFirstObjectByType<InGameCanvas>();
                var go = Instantiate(skillChoicePopupPrefab, canvas.transform);
                var popup = go.GetComponent<SkillChoicePopup>();
                _skillChoicePresenter = new SkillChoicePopupPresenter(popup);
            }

            var playerSkill = PlayerController.Instance.Skill;
            _skillChoicePresenter.Show(playerSkill);
            _modalOpen = true;
        }

        private void OnSkillChosen()
        {
            _modalOpen = false;
        }

        private void ShowGameOverPopup()
        {
            if (_gameOverPresenter == null)
            {
                var canvas = FindFirstObjectByType<InGameCanvas>();
                var go = Instantiate(gameOverPopupPrefab, canvas.transform);
                var popup = go.GetComponent<GameOverPopup>();
                _gameOverPresenter = new GameOverPopupPresenter(popup);
            }

            _gameOverPresenter.Show();
            _modalOpen = true;
        }

        private void ShowGameClearPopup()
        {
            if (_gameClearPresenter == null)
            {
                var canvas = FindFirstObjectByType<InGameCanvas>();
                var go = Instantiate(gameClearPopupPrefab, canvas.transform);
                var popup = go.GetComponent<MapClearPopupView>();
                _gameClearPresenter = new MapClearPopupPresenter(popup);
            }

            _gameClearPresenter.Show();
            _modalOpen = true;
        }

        public void ClearDataInMap()
        {
            _skillChoicePresenter = null;
            _gameOverPresenter = null;
            _gameClearPresenter = null;

            // 일시정지 메뉴는 영구 객체이므로 null 로 만들지 않고 닫기만 한다.
            _pausePresenter?.Hide();
            _modalOpen = false;

            // 인게임 HUD 참조 해제 (씬이 바뀌므로 다음 InGame 로드 때 재탐색)
            _inGameHud = null;
        }

        /// <summary>InGame 씬 로드 시 씬 내의 InGameHud를 탐색해 캐싱한다.</summary>
        private void OnInGameSceneLoaded()
        {
            _inGameHud = FindFirstObjectByType<InGameHud>(FindObjectsInactive.Include);
            if (_inGameHud != null)
                _inGameHud.gameObject.SetActive(false); // FadeIn 직전까지 숨김
            else
                Debug.LogWarning("[UIManager] InGameHud를 찾을 수 없습니다. 인게임 씬 Canvas에 배치했는지 확인하세요.");
        }

        /// <summary>StageManager가 FadeIn 직전에 호출 — 화면이 열릴 때 HUD가 이미 켜진 상태가 된다.</summary>
        public void ShowInGameHud()
        {
            if (_inGameHud != null)
                _inGameHud.gameObject.SetActive(true);
        }

        public async Awaitable FadeOutAsync()
        {
            if (stageTransition)
            {
                stageTransition.gameObject.SetActive(true);
                await stageTransition.FadeOutAsync();
            }
        }

        public async Awaitable FadeInAsync(string stageLabel = null)
        {
            if (stageTransition)
                await stageTransition.FadeInAsync(stageLabel);

            stageTransition.gameObject.SetActive(false);
        }

        public void SetupSettingPopup()
        {
            var lobbyCanvas = FindFirstObjectByType<LobbyCanvas>();
            if (lobbyCanvas == null)
                return;

            var popup = lobbyCanvas.SettingPopup;
            var camera = lobbyCanvas.LobbyCharacterCamera;
            if (popup == null || camera == null)
                return;

            _settingPresenter = new SettingPopupPresenter(popup, camera);

            var openBtn = lobbyCanvas.SettingsButton;
            if (openBtn != null)
                openBtn.onClick.AddListener(() => _settingPresenter.Show());

            var lobbyCameraController = FindAnyObjectByType<LobbyCameraController>();
            if (lobbyCameraController != null)
            {
                _settingPresenter.OnPopupToggled += isOpen =>
                {
                    lobbyCameraController.MapSelectInputBlocked = isOpen;
                    openBtn.gameObject.SetActive(!isOpen);
                };
            }
        }

        public void ClearDataInLobby()
        {
            _settingPresenter = null;
        }
    }
}