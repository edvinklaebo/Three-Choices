public interface IAbility
{
    /// <summary>
    /// Priority for ability execution order within a turn. Lower values execute first.
    /// Typical ranges mirror <see cref="ICombatListener.Priority"/>: 0–99 early, 100–199 standard, 200+ late.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Executes the ability. Use <see cref="CombatContext.DealDamage"/> to apply damage;
    /// the context handles pipeline processing, HP mutation, action creation, and events.
    /// </summary>
    void OnCast(Unit self, Unit target, CombatContext context);
}