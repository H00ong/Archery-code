using Players;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        #region Managers
        [SerializeField] PlayerController playerManager;
        [SerializeField] MapManager MapManager;
        [SerializeField] StageManager StageManager;
        [SerializeField] SoundManager SoundManager;
        [SerializeField] UIManager UIManager;
        [SerializeField] PlayerController PlayerManager;
        [SerializeField] DataManager DataManager;
        [SerializeField] SaveManager SaveManager;
        [SerializeField] InputManager InputManager;
        #endregion


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

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.LevelUp, PauseGame);
            EventBus.Subscribe(EventType.SkillChosen, ResumeGame);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LevelUp, PauseGame);
            EventBus.Unsubscribe(EventType.SkillChosen, ResumeGame);
        }

        void Update()
        {
        
        }

        private void PauseGame() => Time.timeScale = 0f;
        private void ResumeGame() => Time.timeScale = 1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (playerManager == null) playerManager = FindAnyObjectByType<PlayerController>();
            if (MapManager == null) MapManager = FindAnyObjectByType<MapManager>();
            if (StageManager == null) StageManager = FindAnyObjectByType<StageManager>();
            if (SoundManager == null) SoundManager = FindAnyObjectByType<SoundManager>();
            if (UIManager == null) UIManager = FindAnyObjectByType<UIManager>();
            if (PlayerManager == null) PlayerManager = FindAnyObjectByType<PlayerController>();
            if (DataManager == null) DataManager = FindAnyObjectByType<DataManager>();
            if (SaveManager == null) SaveManager = FindAnyObjectByType<SaveManager>();
            if (InputManager == null) InputManager = FindAnyObjectByType<InputManager>();
        }

#endif
    }
}
