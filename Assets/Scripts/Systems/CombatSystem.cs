using System.Collections.Generic;

/// <summary>
/// Backward compatibility wrapper for CombatEngine
/// Maintains static API while delegating to instance-based engine
/// </summary>
public static class CombatSystem
{
    /// <summary>
    /// Run a fight between two units using event-driven combat engine
    /// </summary>
    public static List<ICombatAction> RunFight(Unit attacker, Unit defender)
    {
        var engine = new CombatEngine();
        return engine.RunFight(attacker, defender);
    }
}
