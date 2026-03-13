using Core;
using Interfaces;
using UnityEngine;
using Utils;

/// <summary>
/// Abstract base for passive upgrade definitions.
/// Subclass this to implement a specific passive. Each subclass must override
/// <see cref="CreatePassive"/> to instantiate its concrete <see cref="IPassive"/> implementation
/// and <see cref="PassiveLogName"/> to provide the human-readable name for log messages.
/// The base <see cref="Apply"/> handles attaching the passive and adding it to the unit's list.
/// </summary>
public abstract class PassiveDefinition : UpgradeDefinition
{
    /// <summary>Creates and returns the runtime passive instance for this definition.</summary>
    protected abstract IPassive CreatePassive(Unit unit);

    /// <summary>
    /// Human-readable name emitted in the "Passive Applied: X" log message.
    /// Must match the expected log output in tests that use <c>LogAssert.Expect</c>.
    /// </summary>
    protected abstract string PassiveLogName { get; }

    public override void Apply(Unit unit)
    {
        var passive = CreatePassive(unit);
        Log.Info($"Passive Applied: {PassiveLogName}");
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
