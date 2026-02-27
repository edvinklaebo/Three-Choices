using System.Collections;

/// <summary>
/// Represents a healing action in combat.
/// Shows healing UI with floating text and health bar animation.
/// Stores HP values before and after healing for proper health bar animation.
/// </summary>
public class HealAction : ICombatAction
{
    public Unit Target { get; }
    public int Amount { get; }
    public int TargetHPBefore { get; }
    public int TargetHPAfter { get; }
    public int TargetMaxHP { get; }

    public HealAction(Unit target, int amount, int targetHPBefore, int targetHPAfter, int targetMaxHP)
    {
        Target = target;
        Amount = amount;
        TargetHPBefore = targetHPBefore;
        TargetHPAfter = targetHPAfter;
        TargetMaxHP = targetMaxHP;
    }

    public IEnumerator Play(AnimationContext ctx)
    {
        Log.Info("HealAction.Play", new
        {
            target = Target?.Name ?? "null",
            amount = Amount,
            hpBefore = TargetHPBefore,
            hpAfter = TargetHPAfter
        });

        // Show healing UI with explicit HP values for animation
        ctx.UI.ShowHealing(Target, Amount, TargetHPBefore, TargetHPAfter);
        ctx.UI.AnimateHealthBarToValue(Target, TargetHPBefore, TargetHPAfter);
        ctx.UI.UpdateHealthText(Target, TargetHPAfter, TargetMaxHP);

        // Brief pause to show healing effect
        yield return new UnityEngine.WaitForSeconds(0.2f);
    }
}
