using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Managers
    [SerializeField] PlayerManager playerManager;
    public MapManager MapManager { get; private set; }
    public StageManager StageManager { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public UIManager UIManager { get; private set; }
    public PlayerManager PlayerManager { get; private set; }
    public DataManager DataManager { get; private set; }
    public SaveManager SaveManager { get; private set; }
    public InputManager InputManager { get; private set; }

    #endregion


    private void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else 
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }

    void Start()
    {
        // Initialize managers
        MapManager      = GetComponent<MapManager>();
        StageManager    = GetComponent<StageManager>();
        SoundManager    = GetComponent<SoundManager>();
        UIManager       = GetComponent<UIManager>();
        DataManager     = GetComponent<DataManager>();
        SaveManager     = GetComponent<SaveManager>();
        InputManager    = GetComponent<InputManager>();
    }

    void Update()
    {
        
    }
}
