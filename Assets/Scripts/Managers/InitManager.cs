using System;
using Managers;
using UnityEngine;
public class InitManager : MonoBehaviour
{
    public static InitManager Instance { get; private set; }
    private bool _isInitialized = false;

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
        _isInitialized = true;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && _isInitialized)
        {
            Debug.Log("InitManager: Space key pressed after initialization.");
            StageManager.Instance.Init();
        }
    }

    private async Awaitable InitAsync()
    {
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
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InitManager] Phase 1 failed: {ex.Message}");
            return;
        }

        Debug.Log("[InitManager] Phase 1 complete — DataManager initialized & PoolManager ready.");

        // Phase 2 — Addressables 데이터 병렬 로드
        var mapManager = MapManager.Instance;
        var enemyManager = EnemyManager.Instance;
        var characterManager = CharacterManager.Instance;
        var orbManager = OrbManager.instance;
        var barrelManager = BarrelManager.Instance;
        var skillManager = SkillManager.Instance;

        try
        {
            var mapTask = mapManager.InitAsync();
            var enemyTask = enemyManager.LoadEnemyModulesAsync();
            var characterTask = characterManager.InitAsync();
            var orbTask = orbManager.EnsureOrbSoDictReadyAsync();
            var barrelTask = barrelManager.EnsureBarrelDictReadyAsync();
            var skillTask = skillManager.LoadAllSkillsAsync();

            await mapTask;
            await enemyTask;
            await characterTask;
            await orbTask;
            await barrelTask;
            await skillTask;

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
            return;
        }

        Debug.Log("[InitManager] Phase 2 complete — All Addressables loaded.");

        // Phase 3 — 맵 프리로드
        try
        {
            await mapManager.PreloadMapsAsync();
            destroyCancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("[InitManager] Phase 3 canceled.");
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InitManager] Phase 3 failed: {ex.Message}");
            return;
        }

        Debug.Log("[InitManager] Phase 3 complete — Maps preloaded.");

        PlayerManager.Instance.InitializePlayerData();
    }
}
