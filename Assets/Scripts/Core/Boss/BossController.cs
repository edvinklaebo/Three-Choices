using UnityEngine;

/// <summary>
///     MonoBehaviour that manages the runtime behavior of a boss.
///     Must be attached to the boss prefab referenced in <see cref="BossDefinition.Prefab"/>.
///     Call <see cref="Initialize"/> after spawning the boss, passing the relevant definition.
///     Connect a boss <see cref="Unit"/> health-changed callback to <see cref="OnHealthChanged"/>.
/// </summary>
public class BossController : MonoBehaviour
{
    private BossDefinition _definition;
    private int _currentPhase;

    /// <summary>The active boss definition after <see cref="Initialize"/> has been called.</summary>
    public BossDefinition Definition => _definition;

    /// <summary>Zero-based index of the currently active phase.</summary>
    public int CurrentPhase => _currentPhase;

    /// <summary>
    ///     Sets up the boss with the given definition and enters the first phase.
    ///     Must be called immediately after the boss prefab is instantiated.
    /// </summary>
    public void Initialize(BossDefinition definition)
    {
        if (definition == null)
        {
            Log.Error("[BossController] Cannot initialize with null BossDefinition");
            return;
        }

        if (definition.Phases == null || definition.Phases.Length == 0)
        {
            Log.Error($"[BossController] Boss '{definition.Id}' has no phases defined");
            return;
        }

        _definition = definition;
        ValidatePhaseOrder(definition);
        EnterPhase(0);
    }

    /// <summary>
    ///     Called when the boss unit's HP changes.
    ///     <paramref name="hpPercent"/> must be in the range 0–100.
    ///     Triggers phase transitions when HP falls below the next phase's threshold.
    /// </summary>
    public void OnHealthChanged(float hpPercent)
    {
        if (_definition == null)
            return;

        var nextPhase = _currentPhase + 1;
        if (nextPhase >= _definition.Phases.Length)
            return;

        if (hpPercent <= _definition.Phases[nextPhase].TriggerHPPercent)
            EnterPhase(nextPhase);
    }

    private static void ValidatePhaseOrder(BossDefinition definition)
    {
        var phases = definition.Phases;
        for (var i = 1; i < phases.Length; i++)
        {
            if (phases[i].TriggerHPPercent >= phases[i - 1].TriggerHPPercent)
                Log.Warning($"[BossController] Boss '{definition.Id}' phase {i} trigger ({phases[i].TriggerHPPercent}%) " +
                            $"is not lower than phase {i - 1} ({phases[i - 1].TriggerHPPercent}%). Phases must be ordered from highest to lowest.");
        }
    }

    private void EnterPhase(int phaseIndex)
    {
        _currentPhase = phaseIndex;
        var phase = _definition.Phases[phaseIndex];

        Log.Info($"[BossController] Boss '{_definition.Id}' entering phase {phaseIndex} (trigger: {phase.TriggerHPPercent}%)");

        if (phase.Abilities == null)
            return;

        for (var i = 0; i < phase.Abilities.Length; i++)
        {
            var ability = phase.Abilities[i];
            if (ability == null)
            {
                Log.Warning($"[BossController] Null ability in phase {phaseIndex} of boss '{_definition.Id}'");
                continue;
            }

            ability.Activate(this);
        }
    }
}
