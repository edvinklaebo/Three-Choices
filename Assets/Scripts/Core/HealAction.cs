using System.Collections;

/// <summary>
/// Represents a healing action in combat.
/// Shows healing UI with floating text and health bar animation.
/// Stores HP values before and after healing for proper health bar animation.
/// </summary>
public class HealAction : ICombatAction
{
    public Unit Target { get; private set; }
    public int Amount { get; private set; }
    public int TargetHPBefore { get; private set; }
    public int TargetHPAfter { get; private set; }
    public int TargetMaxHP { get; private set; }

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
        ctx.UI.ShowHealing(Target, Amount);
        ctx.UI.AnimateHealthBarToValue(Target, TargetHPBefore, TargetHPAfter, TargetMaxHP);
        ctx.UI.UpdateHealthText(Target, TargetHPAfter, TargetMaxHP);

        // Brief pause to show healing effect
        yield return new UnityEngine.WaitForSeconds(0.2f);
    }
}
