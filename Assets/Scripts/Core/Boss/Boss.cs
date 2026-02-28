using System;

/// <summary>
///     A <see cref="Unit"/> that represents a boss enemy in combat.
///     Initializes its stats from a <see cref="BossDefinition"/> and exposes
///     the definition so that higher-level systems (e.g. <see cref="BossController"/>)
///     can access phase and ability data.
/// </summary>
[Serializable]
public class Boss : Unit
{
    /// <summary>The definition that describes this boss's phases, rewards, and stats.</summary>
    public BossDefinition Definition { get; }

    public Boss(BossDefinition definition) : base(GetDisplayName(definition))
    {
        Definition = definition;
        Stats = CreateStats(definition.Stats);
    }

    private static string GetDisplayName(BossDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));
        return definition.DisplayName;
    }

    private static Stats CreateStats(Stats source)
    {
        if (source == null)
            return new Stats();

        return new Stats
        {
            MaxHP = source.MaxHP,
            CurrentHP = source.MaxHP,
            AttackPower = source.AttackPower,
            Armor = source.Armor,
            Speed = source.Speed
        };
    }
}
