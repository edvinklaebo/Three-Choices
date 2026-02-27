using System.Collections;

/// <summary>
/// Represents a damage action in combat.
/// Plays attack animation, hit animation, and shows damage UI.
/// Stores HP values before and after damage for proper health bar animation.
/// </summary>
public class DamageAction : ICombatAction
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Amount { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }

    public DamageAction(Unit source, Unit target, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP)
    {
        Source = source;
        Target = target;
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("DamageAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            amount = Amount,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Play attack animation
        yield return ctx.Anim.PlayAttack(Source);

        // Play hit animation
        yield return ctx.Anim.PlayHit(Target);

        // Show damage UI with explicit HP values for animation
        ctx.UI.ShowDamage(Target, Amount, TargetHPBefore, TargetHPAfter, TargetMaxHP);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
