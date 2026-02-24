using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages registration and lifecycle of <see cref="ICombatListener"/> instances for a single
/// combat instance. Listeners are sorted by priority and notified via
/// <see cref="ICombatListener.RegisterHandlers"/> / <see cref="ICombatListener.UnregisterHandlers"/>.
/// </summary>
public class CombatListenerRegistry
{
    private readonly List<ICombatListener> _listeners = new();
    private readonly CombatContext _context;

    public CombatListenerRegistry(CombatContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Register a listener. Duplicate registrations are silently ignored.
    /// Listeners are sorted by <see cref="ICombatListener.Priority"/> after each addition.
    /// </summary>
    public void Register(ICombatListener listener)
    {
        if (_listeners.Contains(listener))
            return;

        _listeners.Add(listener);
        _listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        listener.RegisterHandlers(_context);
    }

    /// <summary>
    /// Unregister all listeners, invoking <see cref="ICombatListener.UnregisterHandlers"/> on each.
    /// </summary>
    public void Clear()
    {
        foreach (var listener in _listeners.ToList())
            Unregister(listener);
        _listeners.Clear();
    }

    private void Unregister(ICombatListener listener)
    {
        if (_listeners.Remove(listener))
            listener.UnregisterHandlers(_context);
    }
}
