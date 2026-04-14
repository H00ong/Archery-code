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
        [Header("Stage Transition")]
        [SerializeField] private UI_StageTransition stageTransition;

        private MapClearPopupPresenter _gameClearPresenter;
        private GameOverPopupPresenter _gameOverPresenter;
        private SkillChoicePopupPresenter _skillChoicePresenter;
        private SettingPopupPresenter _settingPresenter;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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
            EventBus.Subscribe(EventType.TransitionToLobby, ClearDataInMap);
            EventBus.Subscribe(EventType.Retry, ClearDataInMap);
            EventBus.Subscribe(EventType.LevelUp, ShowSkillChoicePopup);
            EventBus.Subscribe(EventType.PlayerDied, ShowGameOverPopup);
            EventBus.Subscribe(EventType.MapCleared, ShowGameClearPopup);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LobbySceneLoaded, SetupSettingPopup);
            EventBus.Unsubscribe(EventType.TransitionToLobby, ClearDataInMap);
            EventBus.Unsubscribe(EventType.Retry, ClearDataInMap);
            EventBus.Unsubscribe(EventType.LevelUp, ShowSkillChoicePopup);
            EventBus.Unsubscribe(EventType.PlayerDied, ShowGameOverPopup);
            EventBus.Unsubscribe(EventType.MapCleared, ShowGameClearPopup);
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
        }

        public void ClearDataInMap()
        {
            _skillChoicePresenter = null;
            _gameOverPresenter = null;
            _gameClearPresenter = null;
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