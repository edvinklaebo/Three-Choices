using System.Collections.Generic;

/// <summary>
/// Records the sequence of <see cref="ICombatAction"/> instances produced during a single
/// combat instance. Actions are appended in resolution order and consumed by the
/// animation / presentation layer after combat ends.
/// </summary>
public class CombatActionLog
{
    private readonly List<ICombatAction> _actions = new();

    public IReadOnlyList<ICombatAction> Actions => _actions;

    /// <summary>
    /// Append a combat action to the log.
    /// </summary>
    public void Add(ICombatAction action) => _actions.Add(action);
}
