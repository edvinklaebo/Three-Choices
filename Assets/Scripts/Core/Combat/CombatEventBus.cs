using System;
using System.Collections.Generic;

/// <summary>
/// Handles event subscription and dispatch for a single combat instance.
/// </summary>
public class CombatEventBus
{
    private readonly Dictionary<Type, List<(Delegate Original, Action<CombatEvent> Wrapper)>> _handlers = new();

    /// <summary>
    /// Subscribe to a specific event type.
    /// </summary>
    public void On<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<(Delegate, Action<CombatEvent>)>();
        _handlers[type].Add((handler, e => handler((TEvent)e)));
    }

    /// <summary>
    /// Unsubscribe from a specific event type.
    /// </summary>
    public void Off<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (_handlers.TryGetValue(type, out var list))
            list.RemoveAll(entry => entry.Original.Equals(handler));
    }

    /// <summary>
    /// Raise a combat event, notifying all subscribed handlers.
    /// </summary>
    public void Raise<TEvent>(TEvent evt) where TEvent : CombatEvent
    {
        var type = typeof(TEvent);
        if (_handlers.TryGetValue(type, out var list))
        {
            var snapshot = list.ToArray();
            foreach (var entry in snapshot)
                entry.Wrapper(evt);
        }
    }

    /// <summary>
    /// Remove all registered handlers.
    /// </summary>
    public void Clear() => _handlers.Clear();
}
