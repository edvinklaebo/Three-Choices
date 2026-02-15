using System.Collections;

/// <summary>
/// Represents a death action in combat.
/// Plays death animation when a unit dies.
/// </summary>
public class DeathAction : ICombatAction
{
    public Unit Target { get; set; }

    public DeathAction(Unit target)
    {
        Target = target;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("DeathAction.Play", new
        {
            target = Target?.Name ?? "null"
        });

        // Play death animation
        yield return ctx.Anim.PlayDeath(Target);

        // Play death sound
        ctx.SFX.PlayDeathSound(Target);
    }
}
