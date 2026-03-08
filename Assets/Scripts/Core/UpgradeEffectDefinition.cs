using UnityEngine;

/// <summary>
///     Base type for all upgrade effects.
///     Provides polymorphic application of an upgrade to a <see cref="Unit" />.
///     Create concrete subclasses (<see cref="StatsDefinition" />, <see cref="PassiveDefinition" />,
///     <see cref="AbilityDefinition" />) to implement specific upgrade behaviour.
/// </summary>
public abstract class UpgradeEffectDefinition : ScriptableObject
{
    public abstract void Apply(Unit unit);
}
