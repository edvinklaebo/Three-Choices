/// <summary>
///     Registers a player <see cref="Unit" /> with the <see cref="CombatLogger" /> and wires up the death event.
/// </summary>
public static class PlayerInitializer
{
    /// <summary>
    ///     Unregisters <paramref name="previous" /> from <see cref="CombatLogger" />, registers
    ///     <paramref name="next" />, and subscribes <paramref name="playerDiedEvent" /> to its death.
    /// </summary>
    public static void Initialize(Unit previous, Unit next, VoidEventChannel playerDiedEvent)
    {
        if (next == null)
        {
            Log.Error("[PlayerInitializer] Cannot initialize null player");
            return;
        }

        CombatLogger.Instance.UnregisterUnit(previous);
        CombatLogger.Instance.RegisterUnit(next);
        next.Died += _ => playerDiedEvent.Raise();
    }
}
