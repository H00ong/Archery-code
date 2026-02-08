using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<EventType, Action> events = new();

    public static void Subscribe(EventType type, Action action)
    {
        events.TryAdd(type, null);
        events[type] += action;
    }

    public static void Unsubscribe(EventType type, Action action)
    {
        if (events.ContainsKey(type))
            events[type] -= action;
    }

    public static void Publish(EventType type)
    {
        if (events.TryGetValue(type, out var action))
            action?.Invoke();
    }
}

public enum EventType
{
    LevelUp,
    SkillChosen,
    StageCombatStarted,
    StageCleared,
    StageLoadingStarted,
}
