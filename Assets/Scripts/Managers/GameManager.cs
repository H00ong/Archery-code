using System.Collections.Generic;
using NUnit.Framework;
using Players;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public enum SceneState
    {
        Lobby,
        InGame,
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public SceneState CurrentState { get; private set; }
        public bool IsInLobby => CurrentState == SceneState.Lobby;
        public bool IsInGame => CurrentState == SceneState.InGame;

        private Dictionary<SceneState, string> _sceneDictionary = new Dictionary<SceneState, string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                Init();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Init()
        {
            CreateDict();
            CurrentState = SceneState.Lobby;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.LevelUp, PauseGame);
            EventBus.Subscribe(EventType.SkillChosen, ResumeGame);
            EventBus.Subscribe(EventType.MapCleared, PauseGame);
            EventBus.Subscribe(EventType.TransitionToLobby, OnTransitionToLobby, 100);
            EventBus.Subscribe(EventType.PlayerDied, PauseGame);
            EventBus.Subscribe(EventType.Retry, OnRetry, 100);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LevelUp, PauseGame);
            EventBus.Unsubscribe(EventType.SkillChosen, ResumeGame);
            EventBus.Unsubscribe(EventType.MapCleared, PauseGame);
            EventBus.Unsubscribe(EventType.TransitionToLobby, OnTransitionToLobby);
            EventBus.Unsubscribe(EventType.PlayerDied, PauseGame);
            EventBus.Unsubscribe(EventType.Retry, OnRetry);
        }

        void Update()
        {
            if(InitManager.Instance.IsLoaded && IsInLobby)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    ChangeScene(SceneState.InGame);
                }
            }
        }

        private void CreateDict()
        {
            _sceneDictionary.Add(SceneState.Lobby, "Lobby");
            _sceneDictionary.Add(SceneState.InGame, "InGame");
        }

        public void ChangeScene(SceneState newState)
        {
            if (_sceneDictionary.TryGetValue(newState, out string sceneName))
            {
                SceneManager.LoadScene(sceneName);
                CurrentState = newState;
            }
            else
            {
                Debug.LogError($"[GameManager] Scene not found for state: {newState}");
            }
        }

        private void OnTransitionToLobby()
        {
            ResumeGame();
            ChangeScene(SceneState.Lobby);
        }

        private void OnRetry()
        {
            ResumeGame();
            ChangeScene(SceneState.InGame);
        }

        private void PauseGame() => Time.timeScale = 0f;
        private void ResumeGame() => Time.timeScale = 1f;
    }
}
