using Game.Stage.Management;
using System;
using System.Collections.Generic;
using Managers;
using Objects;
using Players;
using UnityEngine;

namespace Game.Stage.Management
{
    public enum StageState
    {
        Combat,
        Clear,
        Loading,
    }

    public enum StageCommandType
    {
        EnterPortal,
        AllEnemiesDefeated,
        LoadingComplete,
    }
}

public class StageManager : MonoBehaviour
{
    
    public static StageManager Instance;
    private const int BossStageInterval = 10;
    private bool _initialized = false;
    private bool _allStageCleared = false;
    public bool WaitingForCollectibles { get; private set; }

    private readonly Dictionary<(StageState, StageCommandType), StageState> transitions = new()
    {
        { (StageState.Combat, StageCommandType.AllEnemiesDefeated), StageState.Clear },
        { (StageState.Clear, StageCommandType.EnterPortal), StageState.Loading },
        { (StageState.Loading, StageCommandType.LoadingComplete), StageState.Combat },
    };

    private readonly Dictionary<StageState, EventType> eventTable = new()
    {
        {StageState.Combat, EventType.StageCombatStarted },
        {StageState.Clear, EventType.StageCleared },
        {StageState.Loading, EventType.StageLoadingStarted },
    };

    public int CurrentStageIndex { get; private set; }
    public int TotalStageCountOfMap { get; private set; }
    public List<int> EnemyCountList { get; private set; }
    private StageState CurrentState { get; set; }
    public bool IsBossStage => (CurrentStageIndex % BossStageInterval == 0) && (CurrentStageIndex != 0);
    public bool IsInCombat => CurrentState == StageState.Combat;
    public bool IsInLoading => CurrentState == StageState.Loading;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void UpdateStageIndex()
    {
        CurrentStageIndex++;
        if (CurrentStageIndex >= TotalStageCountOfMap)
        {
            _allStageCleared = true;
            Debug.Log("All stages cleared!");
            EventBus.Publish(EventType.MapCleared);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.StageLoadingStarted, StartStageLoadingSequence, 100);
        EventBus.Subscribe(EventType.StageCleared, ItemCollectActive);
        EventBus.Subscribe(EventType.AllCollectiblesCollected, ItemCollectDeactive);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.StageLoadingStarted, StartStageLoadingSequence);
        EventBus.Unsubscribe(EventType.StageCleared, ItemCollectActive);
        EventBus.Unsubscribe(EventType.AllCollectiblesCollected, ItemCollectDeactive);
    }

    private void ItemCollectActive()
    {
        WaitingForCollectibles = true;
    }

    private void ItemCollectDeactive()
    {
        WaitingForCollectibles = false;
    }

    private void Update()
    {
        if (!WaitingForCollectibles) return;
        if (CollectItem.ActiveCount > 0) return;

        WaitingForCollectibles = false;
        EventBus.Publish(EventType.AllCollectiblesCollected);
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        CurrentStageIndex = -1;

        _allStageCleared = false;
        _initialized = false;

        ChangeState(StageState.Loading);
    }

    public void HandleCommand(StageCommandType cmd)
    {
        if (transitions.TryGetValue((CurrentState, cmd), out var next))
            ChangeState(next);
    }
    
    private void ChangeState(StageState newState)
    {
        CurrentState = newState;

        if (eventTable.TryGetValue(newState, out var action))
            EventBus.Publish(action);
        else
            Debug.LogError($"No action defined for state {newState}");
    }

    private void StartStageLoadingSequence() => StageLoadingAsync().Forget();
    
    private async Awaitable StageLoadingAsync()
    {
        if (!_initialized)
        {
            _initialized = true;

            MapManager.Instance.RefreshMapData();

            var mapData = MapManager.Instance.CurrentMapData;

            TotalStageCountOfMap = mapData.stageCount;
            EnemyCountList = mapData.enemyCountGrid;
        }

        UpdateStageIndex();

        if (_allStageCleared)
            return;

        if (UIManager.Instance)
            await UIManager.Instance.FadeOutAsync();

        try
        {
            await MapManager.Instance.PreloadMapsAsync();
            await CharacterManager.Instance.LoadAndSpawnCharacterAsync();

            destroyCancellationToken.ThrowIfCancellationRequested();

            LoadMap();
            PositionPlayer();
            await SpawnEnemyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[StageManager] Error during stage loading: {e.Message}");
        }

        destroyCancellationToken.ThrowIfCancellationRequested();

        // Fade in — 스테이지 텍스트 표시 후 화면 복원, 그 뒤 Combat 시작
        var label = IsBossStage ? "BOSS" : $"Stage {CurrentStageIndex + 1}";

        if (UIManager.Instance)
            await UIManager.Instance.FadeInAsync(label);

        await Awaitable.WaitForSecondsAsync(.1f);

        HandleCommand(StageCommandType.LoadingComplete);
    }

    private void LoadMap()
    {
        var mapManager = MapManager.Instance;

        mapManager.ActivateRandomMap(IsBossStage);
    }
    
    private void PositionPlayer()
    {
        var mapManager = MapManager.Instance;
        
        var player = PlayerController.Instance;
        var playerContainter = CharacterManager.Instance.PlayerContainer;
        var rb = player.GetComponent<Rigidbody>();
        
        var spawnPos = mapManager.GetPlayerSpawnPoint();
        spawnPos.gameObject.SetActive(true);

        // Rigidbody velocity 초기화 후 위치 설정
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        playerContainter.position = spawnPos.position;
        rb.position = spawnPos.position;
        Physics.SyncTransforms();
        
        CameraController.Instance.SetPosition(spawnPos.position);
        CameraController.Instance.SetTarget(player.transform);

        player.gameObject.SetActive(true);
    }

    private async Awaitable SpawnEnemyAsync()
    {
        var enemyManager = EnemyManager.Instance;
        
        if (IsBossStage)
        {
            var idx = CurrentStageIndex / BossStageInterval;
            await enemyManager.SpawnBossEnemyAsync(idx);
        }
        else
        {
            // 고정 배치 적 스폰
            var predefined = MapManager.Instance.GetPredefinedEnemies();
            if (predefined is { Count: > 0 })
                await enemyManager.SpawnPredefinedEnemiesAsync(predefined);

            // 랜덤 적 스폰
            // TODO : Enemy 생성 count 설정 필요
            var count = 3;
            await enemyManager.SpawnEnemyAsync(count);
        }
    }
}