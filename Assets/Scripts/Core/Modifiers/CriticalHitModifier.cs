using UnityEngine;

/// <summary>
/// Applies critical hit damage based on a chance percentage.
/// Runs as a late-stage multiplier (priority 210).
/// </summary>
public class CriticalHitModifier : IDamageModifier
{
    public int Priority => 210; // Late multiplier, after standard modifiers

    private readonly Unit _owner;
    private readonly float _critChance;
    private readonly float _critMultiplier;

    public CriticalHitModifier(Unit owner, float critChance, float critMultiplier)
    {
        _owner = owner;
        _critChance = Mathf.Clamp01(critChance);
        _critMultiplier = critMultiplier;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;
        if (ctx.IsCritical) return; // Already a crit

        var roll = Random.value;
        if (roll < _critChance)
        {
            ctx.IsCritical = true;
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * _critMultiplier);

            Log.Info("Critical hit!", new
            {
                source = _owner.Name,
                target = ctx.Target?.Name,
                roll,
                critChance = _critChance,
                multiplier = _critMultiplier,
                damage = ctx.FinalValue
            });
        }
    }
}
