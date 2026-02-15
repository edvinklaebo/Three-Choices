using System.Collections;

/// <summary>
/// Represents a status effect action in combat.
/// Used for poison, burn, bleed, and other status effects.
/// </summary>
public class StatusEffectAction : ICombatAction
{
    public Unit Target { get; set; }
    public string EffectName { get; set; }
    public int Amount { get; set; }

    public StatusEffectAction(Unit target, string effectName, int amount = 0)
    {
        Target = target;
        EffectName = effectName;
        Amount = amount;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("StatusEffectAction.Play", new
        {
            target = Target?.Name ?? "null",
            effect = EffectName,
            amount = Amount
        });

        // Play status VFX
        yield return ctx.VFX.PlayStatus(Target, EffectName);

        // Show status UI
        ctx.UI.ShowStatusEffect(Target, EffectName);

        // Play status sound
        ctx.SFX.PlayStatusSound(EffectName);

        // Show damage if applicable (e.g., poison tick)
        if (Amount > 0)
        {
            ctx.UI.ShowPoisonDamage(Target, Amount);
        }
    }
}
