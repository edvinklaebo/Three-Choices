using System.Collections.Generic;
using System.Linq;

namespace Core.Combat
{
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
            this._context = context;
        }

        /// <summary>
        /// Register a listener. Duplicate registrations are silently ignored.
        /// Listeners are sorted by <see cref="ICombatListener.Priority"/> after each addition.
        /// </summary>
        public void Register(ICombatListener listener)
        {
            if (this._listeners.Contains(listener))
                return;

            this._listeners.Add(listener);
            this._listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            listener.RegisterHandlers(this._context);
        }

        /// <summary>
        /// Unregister all listeners, invoking <see cref="ICombatListener.UnregisterHandlers"/> on each.
        /// </summary>
        public void Clear()
        {
            foreach (var listener in this._listeners.ToList())
                Unregister(listener);
            this._listeners.Clear();
        }

        private void Unregister(ICombatListener listener)
        {
            if (this._listeners.Remove(listener))
                listener.UnregisterHandlers(this._context);
        }
    }
}
