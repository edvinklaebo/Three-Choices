using System.Collections.Generic;

/// <summary>
/// Contract for running a fight, enabling RNG injection, difficulty modifiers, and test doubles.
/// </summary>
public interface ICombatSystem
{
    List<ICombatAction> RunFight(Unit attacker, Unit defender);
}
