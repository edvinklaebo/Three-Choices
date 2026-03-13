using Core;
using Interfaces;
using UnityEngine;
using Utils;

/// <summary>
/// Abstract base for passive upgrade definitions.
/// Subclass this to implement a specific passive. Each subclass must override
/// <see cref="CreatePassive"/> to instantiate its concrete <see cref="IPassive"/> implementation.
/// The base <see cref="Apply"/> handles attaching the passive and adding it to the unit's list.
/// </summary>
public abstract class PassiveDefinition : UpgradeDefinition
{
    /// <summary>Creates and returns the runtime passive instance for this definition.</summary>
    protected abstract IPassive CreatePassive(Unit unit);

    /// <summary>
    /// Name used in the "Passive Applied: X" log message.
    /// Defaults to the C# type name of the created passive; override when the passive class name
    /// does not match the desired display name (e.g. <c>PoisonUpgrade</c> → <c>"Poison"</c>).
    /// </summary>
    protected virtual string PassiveLogName => null;

    public override void Apply(Unit unit)
    {
        var passive = CreatePassive(unit);
        var logName = PassiveLogName ?? passive.GetType().Name;
        Log.Info($"Passive Applied: {logName}");
        passive.OnAttach(unit);
        unit.Passives.Add(passive);
    }

#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName)
    {
        id = identifier;
        displayName = soName;
    }
#endif
}
