using System;
using UnityEngine;

/// <summary>
/// Hourglass of Persistence effect.
/// The first time the owner would die, they are revived with a percentage of their max HP instead.
/// Triggers only once per run.
/// </summary>
[Serializable]
public class DeathShield : IPassive
{
    [SerializeField] private float _revivePercent;
    [SerializeField] private bool _triggered;

    [NonSerialized] private Unit _owner;

    public DeathShield(float revivePercent = 0.5f)
    {
        _revivePercent = revivePercent;
    }

    public void OnAttach(Unit owner)
    {
        _owner = owner;
        owner.Died += OnDied;
    }

    public void OnDetach(Unit owner)
    {
        owner.Died -= OnDied;
        _owner = null;
    }

    private void OnDied(Unit unit)
    {
        if (_triggered)
            return;

        _triggered = true;
        var reviveHp = Mathf.CeilToInt(unit.Stats.MaxHP * _revivePercent);

        Log.Info("[DeathShield] Reviving on first death", new
        {
            unit = unit.Name,
            reviveHp,
            maxHP = unit.Stats.MaxHP
        });

        unit.Revive(reviveHp);
    }

    /// <summary>True when the death shield has already been consumed.</summary>
    public bool Triggered => _triggered;
}
