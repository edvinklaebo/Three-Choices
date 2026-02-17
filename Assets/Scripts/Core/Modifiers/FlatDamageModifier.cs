using UnityEngine;

/// <summary>
/// Adds flat bonus damage to attacks.
/// Applied as an early modifier (priority 10) before percentage-based modifiers.
/// </summary>
public class FlatDamageModifier : IDamageModifier
{
    public int Priority => 10; // Very early, adds to base

    private readonly Unit _owner;
    private readonly int _bonusDamage;

    /// <param name="owner">The unit that owns this modifier</param>
    /// <param name="bonusDamage">Flat damage to add to each attack</param>
    public FlatDamageModifier(Unit owner, int bonusDamage)
    {
        _owner = owner;
        _bonusDamage = bonusDamage;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;

        ctx.FinalValue += _bonusDamage;
    }
}
