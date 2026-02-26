/// <summary>
/// Implemented by passives that require a dedicated <see cref="ICombatListener"/>
/// to handle their combat-pipeline integration (e.g. attack queue execution,
/// recursion safety, logging).  The listener is created once per combat by
/// <see cref="CombatEngine"/> and registered with the <see cref="CombatContext"/>.
/// </summary>
public interface ICombatHandlerProvider
{
    ICombatListener CreateCombatHandler(Unit owner);
}
