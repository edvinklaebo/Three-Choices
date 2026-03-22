using Core;
using Interfaces;
using UnityEngine;
using Utils;

/// <summary>
///     Abstract base for all passive upgrade ScriptableObjects.
///     Holds the identity fields (id, displayName, etc.) inherited from
///     <see cref="UpgradeDefinition"/> and provides a shared <see cref="Apply"/>
///     implementation that creates, attaches, and registers the runtime passive.
///     Concrete subclasses (e.g. <see cref="Core.Passives.Definitions.ThornsDefinition"/>,
///     <see cref="Core.Passives.Definitions.RageDefinition"/>) live in
///     Core/Passives/Definitions/ and implement <see cref="CreatePassive"/> to return
///     the appropriate runtime <see cref="IPassive"/> instance.
/// </summary>
public abstract class PassiveDefinition : UpgradeDefinition
{
    /// <summary>Creates a ready-to-use runtime passive seeded from this definition.</summary>
    protected abstract IPassive CreatePassive(Unit unit);

    public override void Apply(Unit unit)
    {
        Log.Info($"Passive Applied: {displayName}");
        var passive = CreatePassive(unit);
        passive.OnAttach(unit);
        unit.Passives.Add(passive);
    }

    public override void Upgrade(IAbility ability)
    {
        throw new System.NotImplementedException();
    }
}