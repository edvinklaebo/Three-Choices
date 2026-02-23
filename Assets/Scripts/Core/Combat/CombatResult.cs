using System.Collections.Generic;

/// <summary>
/// Carries the outcome of a fight computation: the combatants and the list of actions to animate.
/// </summary>
public class CombatResult
{
    public Unit Player { get; }
    public Unit Enemy { get; }
    public List<ICombatAction> Actions { get; }

    public CombatResult(Unit player, Unit enemy, List<ICombatAction> actions)
    {
        Player = player;
        Enemy = enemy;
        Actions = actions ?? new List<ICombatAction>();
    }
}
