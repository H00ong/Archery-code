using System;
using System.Collections.Generic;
public static class EventBus
{
    private static readonly Dictionary<EventType, Action> Events = new();
    
    public static void Subscribe(EventType type, Action action)
    {
        Events.TryAdd(type, null);
        Events[type] += action;
    }
    
    public static void Unsubscribe(EventType type, Action action)
    {
        if (Events.ContainsKey(type))
            Events[type] -= action;
    }
    
    public static void Publish(EventType type)
    {
        if (Events.TryGetValue(type, out var action))
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
    StageLoadingCleared,
}
