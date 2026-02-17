using UnityEngine;

/// <summary>
/// Applies a percentage-based damage multiplier.
/// Applied as a standard modifier (priority 100).
/// </summary>
public class PercentageDamageModifier : IDamageModifier
{
    public int Priority => 100; // Standard priority

    private readonly Unit _owner;
    private readonly float _multiplier;

    /// <param name="owner">The unit that owns this modifier</param>
    /// <param name="multiplier">Damage multiplier (e.g., 1.25 = +25% damage, 0.8 = -20% damage)</param>
    public PercentageDamageModifier(Unit owner, float multiplier)
    {
        _owner = owner;
        _multiplier = multiplier;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;

        ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * _multiplier);
    }
}
