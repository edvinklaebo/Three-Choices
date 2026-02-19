/// <summary>
/// Interface for combat event listeners
/// Passives and abilities implement this to respond to combat events
/// </summary>
public interface ICombatListener
{
    /// <summary>
    /// Priority for listener ordering. Lower values = earlier execution.
    /// 0-99: Early (pre-damage)
    /// 100-199: Normal (damage processing)
    /// 200-299: Late (post-damage effects)
    /// 300+: Very late (cleanup)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Called to register this listener's event handlers
    /// </summary>
    void RegisterHandlers(CombatContext context);

    /// <summary>
    /// Called to unregister this listener's event handlers
    /// </summary>
    void UnregisterHandlers(CombatContext context);
}
