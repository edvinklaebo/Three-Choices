using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles event subscription and dispatch for a single combat instance.
/// </summary>
public class CombatEventBus
{
    private readonly Dictionary<Type, List<Action<CombatEvent>>> _handlers = new();

    /// <summary>
    /// Subscribe to a specific event type.
    /// </summary>
    public void On<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Action<CombatEvent>>();
        _handlers[type].Add(e => handler((TEvent)e));
    }

    /// <summary>
    /// Unsubscribe from a specific event type.
    /// </summary>
    public void Off<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (_handlers.TryGetValue(type, out var list))
            list.RemoveAll(h => h.Target == handler.Target && h.Method == handler.Method);
    }

    /// <summary>
    /// Raise a combat event, notifying all subscribed handlers.
    /// </summary>
    public void Raise<TEvent>(TEvent evt) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (_handlers.TryGetValue(type, out var list))
            foreach (var h in list.ToList())
                h(evt);
    }

    /// <summary>
    /// Remove all registered handlers.
    /// </summary>
    public void Clear() => _handlers.Clear();
}
