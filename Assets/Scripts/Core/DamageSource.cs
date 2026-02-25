/// <summary>
/// Identifies the origin of damage applied via <see cref="CombatContext.ApplyDamage"/>.
/// Used to determine which <see cref="ICombatAction"/> is created for the damage event.
/// </summary>
public enum DamageSource
{
    /// <summary>Normal melee/ranged attack resolved through <see cref="CombatContext.DealDamage"/>.</summary>
    Attack,

    /// <summary>Damage dealt by a status effect tick (e.g. Poison, Burn, Bleed).</summary>
    StatusEffect,

    /// <summary>Damage dealt by an ability (e.g. Fireball). Action creation is left to <see cref="IActionCreator"/>.</summary>
    Ability
}
