using System.Collections;

/// <summary>
/// Represents a status effect action in combat.
/// Used for poison, burn, bleed, and other status effects.
/// </summary>
public class StatusEffectAction : ICombatAction
{
    public Unit Target { get; }
    public string EffectName { get; }
    public int Amount { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }

    public StatusEffectAction(Unit target, string effectName, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP)
    {
        Target = target;
        EffectName = effectName;
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("StatusEffectAction.Play", new
        {
            target = Target?.Name ?? "null",
            effect = EffectName,
            amount = Amount,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Play status VFX
        yield return ctx.VFX.PlayStatus(Target, EffectName);

        // Show status UI
        ctx.UI.ShowStatusEffect(Target, EffectName);

        // Play status sound
        ctx.SFX.PlayStatusSound(EffectName);

        // Show damage if applicable (e.g., poison tick, bleed, burn)
        if (Amount > 0 && TargetMaxHP > 0)
        {
            var damageType = GetDamageTypeForEffect(EffectName);
            ctx.UI.ShowDamage(Target, Amount, TargetHPBefore, TargetHPAfter, TargetMaxHP, damageType);
        }
    }

    /// <summary>
    /// Map effect name to appropriate damage type for visual feedback.
    /// </summary>
    private DamageType GetDamageTypeForEffect(string effectName)
    {
        return effectName?.ToLower() switch
        {
            "poison" => DamageType.Poison,
            "bleed" => DamageType.Bleed,
            "burn" => DamageType.Burn,
            _ => DamageType.Physical
        };
    }
}
