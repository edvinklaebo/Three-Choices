using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Combat context managing events, listeners, and actions for a single combat instance
/// </summary>
public class CombatContext
{
    private readonly List<ICombatAction> _actions = new();
    private readonly List<ICombatListener> _listeners = new();
    private readonly Dictionary<Type, List<Action<CombatEvent>>> _eventHandlers = new();

    public IReadOnlyList<ICombatAction> Actions => _actions;

    /// <summary>
    /// Register a listener for combat events
    /// </summary>
    public void RegisterListener(ICombatListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
            // Sort by priority after adding
            _listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            listener.RegisterHandlers(this);
        }
    }

    /// <summary>
    /// Unregister a listener from combat events
    /// </summary>
    public void UnregisterListener(ICombatListener listener)
    {
        if (_listeners.Remove(listener))
        {
            listener.UnregisterHandlers(this);
        }
    }

    /// <summary>
    /// Subscribe to a specific event type
    /// </summary>
    public void On<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = new List<Action<CombatEvent>>();
        }
        _eventHandlers[eventType].Add(e => handler((TEvent)e));
    }

    /// <summary>
    /// Unsubscribe from a specific event type
    /// </summary>
    public void Off<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (_eventHandlers.ContainsKey(eventType))
        {
            // Find and remove the matching handler
            _eventHandlers[eventType].RemoveAll(h => h.Target == handler.Target && h.Method == handler.Method);
        }
    }

    /// <summary>
    /// Raise a combat event, notifying all subscribed handlers
    /// </summary>
    public void Raise<TEvent>(TEvent evt) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (_eventHandlers.ContainsKey(eventType))
        {
            foreach (var handler in _eventHandlers[eventType].ToList())
            {
                handler(evt);
            }
        }
    }

    /// <summary>
    /// Add a combat action to the action log
    /// </summary>
    public void AddAction(ICombatAction action)
    {
        _actions.Add(action);
    }

    /// <summary>
    /// Clear all registered listeners and handlers
    /// </summary>
    public void Clear()
    {
        foreach (var listener in _listeners.ToList())
        {
            UnregisterListener(listener);
        }
        _listeners.Clear();
        _eventHandlers.Clear();
    }
}
