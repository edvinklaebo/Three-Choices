using System.Collections;

/// <summary>
/// Represents a fireball ability action in combat.
/// Plays fireball visual from source to target, then shows damage and applies burn.
/// </summary>
public class FireballAction : ICombatAction
{
    public Unit Source { get; set; }
    public Unit Target { get; set; }
    public int Damage { get; set; }
    public int TargetHPBefore { get; set; }
    public int TargetHPAfter { get; set; }
    public int TargetMaxHP { get; set; }

    public FireballAction(Unit source, Unit target, int damage, int targetHPBefore, int targetHPAfter, int targetMaxHP)
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
        Log.Info("FireballAction.Play", new
        {
            source = Source?.Name ?? "null",
            target = Target?.Name ?? "null",
            damage = Damage,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Play fireball visual effect (placeholder - similar to attack lunge)
        // In the future, this could be replaced with a proper projectile animation
        yield return ctx.AnimationService.PlayAttack(Source);

        // Play hit effect on target
        yield return ctx.AnimationService.PlayHit(Target);

        // Show damage UI with explicit HP values for animation
        ctx.UIService.ShowDamage(Target, Damage, TargetHPBefore, TargetHPAfter, TargetMaxHP, DamageType.Burn);

        // Play fire/hit sound
        ctx.SFXService.PlayHitSound(Target);
    }
}
