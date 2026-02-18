using System;
using UnityEngine;

/// <summary>
/// Increases damage based on missing health percentage.
/// Applied as a late-stage multiplier (priority 200).
/// </summary>
[Serializable]
public class Rage : Passive, IDamageModifier
{
    public int Priority => 200; // Late-stage multiplier

    public Rage(Unit owner)
    {
        Owner = owner;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != Owner) return;

        var missingHpPercent =
            1f - (float)Owner.Stats.CurrentHP / Owner.Stats.MaxHP;

        var bonus = 1f + missingHpPercent; // up to +100%

        ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);
    }
}