using System;
using Managers;
using UI;
using UnityEngine;
public class InitManager : MonoBehaviour
{
    public static InitManager Instance { get; private set; }
    public bool IsLoaded { get; private set; }

    [Header("Loading UI")]
    [SerializeField] private UI_ProgressBar progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitAsync();
    }

    private void Update()
    {
        bool canTransition = IsLoaded
                             && Input.GetKeyDown(KeyCode.Space)
                             && GameManager.Instance.CurrentState == SceneState.Loading;

        if (canTransition)
        {
            Debug.Log("[InitManager] Space key pressed — Transitioning to Lobby scene.");
            GameManager.Instance.ChangeScene(SceneState.Lobby);
        }
    }

    private async Awaitable InitAsync()
    {
        IsLoaded = false;
        SetProgress(0f, "Initializing...");

        // Phase 1 — DataManager & PoolManager (싱글톤 Awake로 이미 준비됨, 참조만 확보)
        var dataManager = DataManager.Instance;
        var poolManager = PoolManager.Instance;

        if (!dataManager || !poolManager)
        {
            Debug.LogError("[InitManager] DataManager or PoolManager not found. Check scene setup.");
            return;
        }

        try
        {
            dataManager.Init();
            PlayerManager.Instance.InitializePlayerData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InitManager] Phase 1 failed: {ex.Message}");
            return;
        }

        SetProgress(0.1f, "DataManager initialized");
        Debug.Log("[InitManager] Phase 1 complete — DataManager initialized & PoolManager ready.");

        // Phase 2 — Addressables 데이터 순차 로드 (진행률 추적)
        var mapManager = MapManager.Instance;
        var characterManager = CharacterManager.Instance;
        var orbManager = OrbManager.Instance;
        var barrelManager = BarrelManager.Instance;
        var skillManager = SkillManager.Instance;
        var lobbyMapManager = LobbyMapManager.Instance;
        var lobbyCharacterManager = LobbyCharacterManager.Instance;

        try
        {
            SetProgress(0.15f, "Loading map data...");
            await mapManager.LoadMapConfigAsync();
            Debug.Log("[InitManager] ✓ MapManager done");

            SetProgress(0.35f, "Loading character data...");
            await characterManager.LoadCharacterIdentitiesAsync();
            Debug.Log("[InitManager] ✓ CharacterManager done");

            SetProgress(0.55f, "Loading orb data...");
            await orbManager.LoadOrbConfigurationsAsync();
            Debug.Log("[InitManager] ✓ OrbManager done");

            SetProgress(0.70f, "Loading barrel data...");
            await barrelManager.LoadBarrelAssetsAsync();
            Debug.Log("[InitManager] ✓ BarrelManager done");

            SetProgress(0.85f, "Loading skill data...");
            await skillManager.LoadAllSkillsAsync();
            Debug.Log("[InitManager] ✓ SkillManager done");

            SetProgress(0.90f, "Loading lobby maps...");
            await lobbyMapManager.InitLobbyAsync();
            Debug.Log("[InitManager] ✓ LobbyMapManager done");

            SetProgress(0.95f, "Loading lobby characters...");
            await lobbyCharacterManager.InitLobbyAsync();
            Debug.Log("[InitManager] ✓ LobbyCharacterManager done");

            destroyCancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("[InitManager] Phase 2 canceled.");
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InitManager] Phase 2 failed: {ex.Message}");
            Debug.LogException(ex); // 상세 스택 트레이스 로그 추가
            return;
        }

        SetProgress(1f, "Loading complete!");
        IsLoaded = true;
    }

    private void SetProgress(float progress, string status)
    {
        if (!progressBar) return;
        progressBar.SetProgress(progress);
        progressBar.SetStatus(status);
    }
}
