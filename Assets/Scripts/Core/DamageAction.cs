using System.Collections;

/// <summary>
/// Represents a damage action in combat.
/// Plays attack animation, hit animation, and shows damage UI.
/// </summary>
public class DamageAction : ICombatAction
{
    public Unit Source { get; set; }
    public Unit Target { get; set; }
    public int Amount { get; set; }

    public DamageAction(Unit source, Unit target, int amount)
    {
        Source = source;
        Target = target;
        Amount = amount;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("DamageAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            amount = Amount
        });

        // Play attack animation
        yield return ctx.Anim.PlayAttack(Source);

        // Play hit animation
        yield return ctx.Anim.PlayHit(Target);

        // Show damage UI
        ctx.UI.ShowDamage(Target, Amount);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
