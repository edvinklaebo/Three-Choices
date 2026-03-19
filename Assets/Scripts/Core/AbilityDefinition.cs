using Core;
using Interfaces;

/// <summary>
///     Abstract base for all ability upgrade ScriptableObjects.
///     Concrete subclasses (e.g. <see cref="Core.Abilities.Definitions.FireballDefinition"/>,
///     <see cref="Core.Abilities.Definitions.ArcaneMissilesDefinition"/>) live in
///     Core/Abilities/Definitions/ and override <see cref="UpgradeDefinition.Apply"/> to handle
///     first-pickup creation and duplicate stacking.
/// </summary>
public abstract class AbilityDefinition : UpgradeDefinition
{
    /// <summary>
    ///     Returns the first ability of type <typeparamref name="T"/> on the unit, or null.
    ///     Use this in <see cref="UpgradeDefinition.Apply"/> to detect whether the ability is
    ///     already owned before creating a new instance.
    /// </summary>
    protected static T FindExistingAbility<T>(Unit unit) where T : class, IAbility
    {
        foreach (var ability in unit.Abilities)
            if (ability is T found)
                return found;
        return null;
    }
}