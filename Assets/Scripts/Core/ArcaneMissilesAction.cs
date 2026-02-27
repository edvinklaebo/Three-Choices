using System.Collections;

/// <summary>
/// Represents an Arcane Missiles ability action in combat.
/// Plays missile visuals from source to target for each missile hit, then shows damage.
/// </summary>
public class ArcaneMissilesAction : ICombatAction
{
    public Unit Source { get; }
    public Unit Target { get; }
    public int Damage { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }

    public ArcaneMissilesAction(Unit source, Unit target, int damage, int targetHPBefore, int targetHPAfter, int targetMaxHP)
    {
        Source = source;
        Target = target;
        Damage = damage;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("ArcaneMissilesAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            damage = Damage,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Play attack animation from source
        yield return ctx.Anim.PlayAttack(Source);

        // Play hit effect on target
        yield return ctx.Anim.PlayHit(Target);

        // Show damage UI with arcane color
        ctx.UI.ShowDamage(Target, Damage, TargetHPBefore, TargetHPAfter, TargetMaxHP, DamageType.Arcane);

        // Play hit sound
        ctx.SFX.PlayHitSound(Target);
    }
}
