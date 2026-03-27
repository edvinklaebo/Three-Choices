using System;
using System.Collections;
using Core;
using Interfaces;
using UnityEngine;
using Utils;

/// <summary>
/// Represents a Bow Shot ability action in combat.
/// Animates a projectile from the source center to the target center
/// instead of the default lunge animation used by DamageAction.
/// </summary>
public class BowShotAction : ICombatAction
{
    private Unit Source { get; }
    private Unit Target { get; }
    private int Amount { get; }
    private int TargetHPBefore { get; }
    private int TargetHPAfter { get; }
    private int TargetMaxHP { get; }
    private Sprite Sprite { get; }

    public BowShotAction(Unit source, Unit target, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP, Sprite sprite = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
        Sprite = sprite;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("BowShotAction.Play", new
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

        // Show physical damage UI
        ctx.UI.ShowDamage(Target, Amount, TargetHPBefore, TargetHPAfter, TargetMaxHP);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
