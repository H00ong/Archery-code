using System;
using Managers;
using UnityEngine;
public class InitManager : MonoBehaviour
{
    public static InitManager Instance { get; private set; }
    public bool IsLoaded { get; private set; }

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

    private async Awaitable InitAsync()
    {
        IsLoaded = false;

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
        var characterManager = CharacterManager.Instance;
        var orbManager = OrbManager.instance;
        var barrelManager = BarrelManager.Instance;
        var skillManager = SkillManager.Instance;

        try
        {
            var mapTask = mapManager.LoadMapConfigAsync();
            var characterTask = characterManager.LoadCharacterIdentitiesAsync();
            var orbTask = orbManager.LoadOrbConfigurationsAsync();
            var barrelTask = barrelManager.LoadBarrelAssetsAsync();
            var skillTask = skillManager.LoadAllSkillsAsync();

            await mapTask;
            await characterTask;
            await orbTask;
            //await barrelTask;
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
            Debug.LogException(ex); // 상세 스택 트레이스 로그 추가
            return;
        }

        Debug.Log("[InitManager] Phase 2 complete — All Addressables loaded.");

        PlayerManager.Instance.InitializePlayerData();

        IsLoaded = true;
    }
}
