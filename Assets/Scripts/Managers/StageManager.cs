using Game.Stage.Management;
using System;
using System.Collections.Generic;
using Managers;
using Map;
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

    private readonly Dictionary<(StageState, StageCommandType), StageState> transitions = new()
    {
        { (StageState.Combat, StageCommandType.AllEnemiesDefeated), StageState.Clear },
        { (StageState.Clear, StageCommandType.EnterPortal), StageState.Loading },
        { (StageState.Loading, StageCommandType.LoadingComplete), StageState.Combat },
    };

    private readonly Dictionary<StageState, Action> actionTable = new()
    {
        {StageState.Combat, () => EventBus.Publish(EventType.StageCombatStarted) },
        {StageState.Clear, () => EventBus.Publish(EventType.StageCleared) },
        {StageState.Loading, () => EventBus.Publish(EventType.StageLoadingStarted) },
    };

    public int CurrentStageIndex { get; private set; }
    public int TotalStageCountOfMap { get; private set; }
    public List<int> EnemyCountList { get; private set; }
    private StageState CurrentState { get; set; }
    public bool IsBossStage => (CurrentStageIndex % 10 == 0) && (CurrentStageIndex != 0);
    public bool IsInCombat => CurrentState == StageState.Combat;

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
        CurrentStageIndex = (CurrentStageIndex + 1) % 10;
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.StageLoadingStarted, StartStageLoadingSequence);
        EventBus.Subscribe(EventType.StageCleared, UpdateStageIndex);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.StageLoadingStarted, StartStageLoadingSequence);
        EventBus.Unsubscribe(EventType.StageCleared, UpdateStageIndex);
    }

    public void Init()
    {
        CurrentStageIndex = 0;

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

        if (actionTable.TryGetValue(newState, out var action))
            action?.Invoke();
        else
            Debug.LogError($"No action defined for state {newState}");
    }

    private void StartStageLoadingSequence() => StageLoadingAsync().Forget();
    
    private async Awaitable StageLoadingAsync()
    {
        await Awaitable.NextFrameAsync();

        var mapData = MapManager.Instance.CurrentMapData;
        
        TotalStageCountOfMap = mapData.stageCount;
        EnemyCountList = mapData.enemyCountGrid;

        try
        {
            await CharacterManager.Instance.LoadAndSpawnCharacterAsync();

            LoadMap();
            PositionPlayer();
            await SpawnEnemyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[StageManager] Error during stage loading: {e.Message}");
        }

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
        
        var spawnPos = mapManager.GetPlayerSpawnPoint();
        spawnPos.gameObject.SetActive(true);

        playerContainter.position = spawnPos.position;
        player.transform.position = spawnPos.position;
        
        CameraController.Instance.SetPosition(spawnPos.position);

        player.gameObject.SetActive(true);
    }

    private async Awaitable SpawnEnemyAsync()
    {
        var enemyManager = EnemyManager.Instance;
        
        if (IsBossStage)
        {
            var idx = CurrentStageIndex / 10;
            await enemyManager.SpawnBossEnemyAsync(idx);
        }
        else
        {
            // TODO : Enemy 생성 count 설정 필요
            var count = 1;
            await enemyManager.SpawnEnemyAsync(count);
        }
    }
}