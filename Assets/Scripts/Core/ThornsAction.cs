using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a Thorns passive reflect in combat.
/// Skips the attacker lunge and instead shakes the unit that takes the reflected damage,
/// making it visually clear the damage is a passive counter rather than an active attack.
/// </summary>
public class ThornsAction : ICombatAction
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Amount { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }

    public ThornsAction(Unit source, Unit target, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP)
    {
        Debug.Assert(target != null, "ThornsAction: target must not be null");
        Source = source;
        Target = target;
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("ThornsAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            amount = Amount,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Shake the target to show the reflected damage landing
        yield return ctx.Anim.PlayShake(Target);

        // Show damage number and health bar update
        ctx.UI.ShowDamage(Target, Amount, TargetHPBefore, TargetHPAfter, TargetMaxHP);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
