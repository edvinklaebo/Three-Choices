/// <summary>
/// Applies modifications to damage calculations during combat.
/// Modifiers are applied in priority order (lower values = higher priority).
/// </summary>
public interface IDamageModifier
{
    /// <summary>
    /// Priority determines order of modifier application.
    /// Lower values execute first. Typical ranges:
    /// - 0-99: Early modifiers (base stat changes, armor penetration)
    /// - 100-199: Standard modifiers (most buffs/debuffs)
    /// - 200-299: Late modifiers (final multipliers, crit)
    /// - 300+: Post-processing (minimum damage, caps)
    /// </summary>
    int Priority { get; }

    void Modify(DamageContext context);
}