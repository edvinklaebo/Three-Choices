using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a Fireball ability action in combat.
/// Animates a projectile from the source center to the target center
/// instead of the default lunge animation used by DamageAction.
/// </summary>
public class FireballAction : ICombatAction
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Amount { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }
    public Sprite Sprite { get; }

    public FireballAction(Unit source, Unit target, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP, Sprite sprite = null)
    {
        Source = source;
        Target = target;
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
        Sprite = sprite;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("FireballAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            amount = Amount,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Animate projectile from source center to target center (no lunge)
        yield return ctx.Anim.PlayProjectile(Source, Target, Sprite);

        // Play hit reaction on target
        yield return ctx.Anim.PlayHit(Target);

        // Show fire damage UI
        ctx.UI.ShowDamage(Target, Amount, TargetHPBefore, TargetHPAfter, TargetMaxHP, DamageType.Burn);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
