using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<EventType, List<(int Priority, Action Callback)>> _subscribers = new();

    // priority 파라미터 추가 (값이 클수록 나중에 실행)
    public static void Subscribe(EventType type, Action callback, int priority = 0)
    {
        if (!_subscribers.ContainsKey(type))
            _subscribers[type] = new List<(int, Action)>();

        _subscribers[type].Add((priority, callback));
        _subscribers[type].Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public static void Unsubscribe(EventType type, Action callback)
    {
        if (_subscribers.TryGetValue(type, out var list))
            list.RemoveAll(x => x.Callback == callback);
    }

    public static void Publish(EventType type)
    {
        if (!_subscribers.TryGetValue(type, out var list) || list.Count == 0)
            return;

        var snapshot = list.ToArray();
        foreach (var subscriber in snapshot)
            subscriber.Callback?.Invoke();
    }
}

public enum EventType
{
    LevelUp,
    SkillChosen,
    StageCombatStarted,
    StageCleared,
    StageLoadingStarted,
    MapCleared,
    PlayerSpawned,
    PlayerDied,
    Retry,
    TransitionToLobby,
    AllCollectiblesCollected,
}
