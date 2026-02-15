using UnityEngine;

public class Rage : Passive, IDamageModifier
{
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