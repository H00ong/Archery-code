using Game.Stage.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
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
    
    public enum LoadingTask
    {
        GenerateMap,
        SpawnEnemies,
        PositionPlayer
    }
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    
    private readonly HashSet<LoadingTask> pendingLoadingTasks = new();
    
    private readonly Dictionary<(StageState, StageCommandType), StageState> transitions = new()
    {
        { (StageState.Clear, StageCommandType.EnterPortal), StageState.Loading },
        { (StageState.Combat, StageCommandType.AllEnemiesDefeated), StageState.Clear },
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

    private void UpdateStageIndex() => CurrentStageIndex++;

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

    public void Init(MapData mapData)
    {
        CurrentStageIndex = 0;
        
        TotalStageCountOfMap = mapData.stageCount;
        EnemyCountList = mapData.enemyCountGrid;

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

    private void StartStageLoadingSequence() => StartCoroutine(StageLoadingCoroutine());
    
    private IEnumerator StageLoadingCoroutine()
    {
        LoadMap();

        // 맵 데이터 캐싱 초기화 (스테이지별 배율 적용)
        var mapData = MapManager.Instance.CurrentMapData;
        EnemyManager.Instance.SetUpEnemyEffects(mapData, CurrentStageIndex);

        // Player setting
        PositionPlayer();
        
        // EnemySetting
        yield return SpawnEnemy();

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
        
        var player = PlayerController.Instance.transform;
        var playerContainter = player.parent;
        
        var spawnPos = mapManager.GetPlayerSpawnPoint();
        spawnPos.gameObject.SetActive(true);

        playerContainter.position = spawnPos.position;
        player.position = spawnPos.position;
    }

    private IEnumerator SpawnEnemy()
    {
        var enemyManager = EnemyManager.Instance;
        
        if (IsBossStage)
        {
            var idx = CurrentStageIndex / 10;
            yield return enemyManager.SpawnBossEnemey(idx); 
        }
        else
        {
            // TODO : Enemy 생성 count 설정 필요
            var count = 1;
            yield return enemyManager.SpawnEnemy(count);
        }
    }
}
