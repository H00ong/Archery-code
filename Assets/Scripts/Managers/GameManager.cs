using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Managers
    [SerializeField] PlayerManager playerManager;
    [SerializeField] MapManager MapManager;
    [SerializeField] StageManager StageManager;
    [SerializeField] SoundManager SoundManager;
    [SerializeField] UIManager UIManager;
    [SerializeField] PlayerManager PlayerManager;
    [SerializeField] DataManager DataManager;
    [SerializeField] SaveManager SaveManager;
    [SerializeField] InputManager InputManager;

    #endregion


    private void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
            
            // Initialize managers
        }
        else 
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }

    private void OnEnable()
    {
        LevelUpFlow.OnLevelUp += PauseGame;
        LevelUpFlow.OnSkillChosen += ResumeGame;
    }

    private void OnDisable()
    {
        LevelUpFlow.OnLevelUp -= PauseGame;
        LevelUpFlow.OnSkillChosen -= ResumeGame;
    }

    void Update()
    {
        
    }

    private void PauseGame() => Time.timeScale = 0f;
    private void ResumeGame() => Time.timeScale = 1f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerManager == null) playerManager = FindAnyObjectByType<PlayerManager>();
        if (MapManager == null) MapManager = FindAnyObjectByType<MapManager>();
        if (StageManager == null) StageManager = FindAnyObjectByType<StageManager>();
        if (SoundManager == null) SoundManager = FindAnyObjectByType<SoundManager>();
        if (UIManager == null) UIManager = FindAnyObjectByType<UIManager>();
        if (PlayerManager == null) PlayerManager = FindAnyObjectByType<PlayerManager>();
        if (DataManager == null) DataManager = FindAnyObjectByType<DataManager>();
        if (SaveManager == null) SaveManager = FindAnyObjectByType<SaveManager>();
        if (InputManager == null) InputManager = FindAnyObjectByType<InputManager>();
    }

#endif
}
