using System;
using System.Linq;

/// <summary>
/// Runs the damage resolution pipeline for a single combat instance.
/// Responsible for phase orchestration, HP mutation, and pending-effect application
/// (healing, resource gain, status effects).
/// </summary>
public class CombatDamageResolver
{
    private readonly CombatEventBus _eventBus;
    private readonly CombatActionLog _actionLog;

    public CombatDamageResolver(CombatEventBus eventBus, CombatActionLog actionLog)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _actionLog = actionLog ?? throw new ArgumentNullException(nameof(actionLog));
    }

    /// <summary>
    /// Deals damage from <paramref name="source"/> to <paramref name="target"/> through all
    /// combat phases. This is the single point where HP is mutated and healing/statuses are
    /// applied.
    /// <para>
    /// Pass <paramref name="onHitStatus"/> to queue a status effect scaled to the final damage
    /// dealt (e.g. burn = 50% of final damage). The factory receives the resolved damage value
    /// and its returned status is applied through the normal
    /// <see cref="CombatPhase.StatusApplication"/> phase.
    /// </para>
    /// <para>
    /// Pass <paramref name="effectId"/> when damage originates from a status effect tick.
    /// A <see cref="StatusEffectAction"/> is recorded instead of a <see cref="DamageAction"/>.
    /// </para>
    /// </summary>
    public void DealDamage(Unit source, Unit target, int baseDamage,
        Func<int, IStatusEffect> onHitStatus = null, string effectId = null)
    {
        if (target == null || target.IsDead) return;

        var ctx = new DamageContext(source, target, baseDamage);

        ExecutePhase(CombatPhase.PreAction, ctx);
        if (ctx.Cancelled) return;

        DamagePipeline.Process(ctx);
        ctx.ModifiedDamage = ctx.FinalValue;

        ExecutePhase(CombatPhase.DamageCalculation, ctx);
        ExecutePhase(CombatPhase.Mitigation, ctx);

        ctx.FinalDamage = ctx.ModifiedDamage;

        if (onHitStatus != null)
            ctx.PendingStatuses.Add(onHitStatus(ctx.FinalDamage));

        var hpBefore = target.Stats.CurrentHP;
        var maxHP = target.Stats.MaxHP;
        target.ApplyDamage(source, ctx.FinalDamage);
        var hpAfter = target.Stats.CurrentHP;

        _actionLog.Add(effectId != null
            ? (ICombatAction)new StatusEffectAction(target, effectId, ctx.FinalDamage, hpBefore, hpAfter, maxHP)
            : new DamageAction(source, target, ctx.FinalDamage, hpBefore, hpAfter, maxHP));

        _eventBus.Raise(new OnHitEvent(source, target, ctx.FinalDamage));

        ExecutePhase(CombatPhase.Healing, ctx);
        // ResourceGain and StatusApplication use immediate application: their phase lets listeners
        // queue values, then Apply* runs right after so effects see a consistent state.
        ExecutePhase(CombatPhase.ResourceGain, ctx);
        ApplyResourceGain(ctx);
        ExecutePhase(CombatPhase.StatusApplication, ctx);
        ApplyStatuses(ctx);
        // PostResolve runs last (e.g. Lifesteal queues PendingHealing here), then ApplyHealing
        // runs so all accumulated healing from Healing + PostResolve phases is applied together.
        ExecutePhase(CombatPhase.PostResolve, ctx);
        ApplyHealing(ctx);

        if (target.IsDead && _actionLog.Actions.OfType<DeathAction>().All(a => a.Target != target))
            _actionLog.Add(new DeathAction(target));
    }

    private void ExecutePhase(CombatPhase phase, DamageContext context)
    {
        _eventBus.Raise(new DamagePhaseEvent(phase, context));
    }

    private void ApplyHealing(DamageContext context)
    {
        if (context.PendingHealing <= 0) return;

        var hpBefore = context.Source.Stats.CurrentHP;
        context.Source.Heal(context.PendingHealing);
        var hpAfter = context.Source.Stats.CurrentHP;

        _actionLog.Add(new HealAction(context.Source, context.PendingHealing, hpBefore, hpAfter, context.Source.Stats.MaxHP));
    }

    private void ApplyResourceGain(DamageContext context)
    {
        if (context.PendingResourceGain > 0)
        {
            Log.Warning("ApplyResourceGain: resource system not yet implemented", new
            {
                source = context.Source?.Name,
                context.PendingResourceGain
            });
        }
    }

    private void ApplyStatuses(DamageContext context)
    {
        foreach (var status in context.PendingStatuses)
            context.Target.ApplyStatus(status);
    }
}

