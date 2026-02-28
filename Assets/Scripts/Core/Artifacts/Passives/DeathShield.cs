using System;
using UnityEngine;

/// <summary>
/// Hourglass of Persistence effect.
/// Subscribes to <see cref="Unit.Dying"/> (before death) and cancels the first fatal hit,
/// restoring the unit to a percentage of max HP.
/// Because it uses <see cref="IPassive.Priority"/> = 0, it is attached (and subscribes) before
/// all other passives, ensuring it intercepts death before any other Dying subscriber.
/// Triggers only once per run.
/// </summary>
[Serializable]
public class DeathShield : IPassive
{
    [SerializeField] private float _revivePercent;
    [SerializeField] private bool _triggered;

    /// <summary>Runs first among passives so death is canceled before other subscribers see it.</summary>
    public int Priority => 0;

    public DeathShield(float revivePercent = 0.5f)
    {
        _revivePercent = revivePercent;
    }

    public void OnAttach(Unit owner)
    {
        owner.Dying += OnDying;
    }

    public void OnDetach(Unit owner)
    {
        owner.Dying -= OnDying;
    }

    private void OnDying(Unit unit, DyingEventArgs args)
    {
        // Check args.Cancelled defensively: a future higher-priority passive (Priority < 0) might
        // also intercept death. Respecting an existing cancellation avoids overwriting its ReviveHp.
        if (_triggered || args.Cancelled)
            return;

        _triggered = true;
        args.Cancelled = true;
        args.ReviveHp = Mathf.CeilToInt(unit.Stats.MaxHP * _revivePercent);

        Log.Info("[DeathShield] Death cancelled, reviving", new
        {
            unit = unit.Name,
            reviveHp = args.ReviveHp,
            maxHP = unit.Stats.MaxHP
        });
    }

    /// <summary>True when the death shield has already been consumed.</summary>
    public bool Triggered => _triggered;
}
