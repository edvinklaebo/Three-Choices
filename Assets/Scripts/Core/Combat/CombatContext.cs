using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Combat context managing events, listeners, and actions for a single combat instance
/// </summary>
public class CombatContext
{
    private readonly List<ICombatAction> _actions = new();
    private readonly List<ICombatListener> _listeners = new();
    private readonly Dictionary<Type, List<Action<CombatEvent>>> _eventHandlers = new();

    public IReadOnlyList<ICombatAction> Actions => _actions;

    /// <summary>
    /// Register a listener for combat events
    /// </summary>
    public void RegisterListener(ICombatListener listener)
    {
        if (_listeners.Contains(listener)) 
            return;
        
        _listeners.Add(listener);
        // Sort by priority after adding
        _listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        listener.RegisterHandlers(this);
    }

    /// <summary>
    /// Unregister a listener from combat events
    /// </summary>
    private void UnregisterListener(ICombatListener listener)
    {
        if (_listeners.Remove(listener))
        {
            listener.UnregisterHandlers(this);
        }
    }

    /// <summary>
    /// Subscribe to a specific event type
    /// </summary>
    public void On<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = new List<Action<CombatEvent>>();
        }
        _eventHandlers[eventType].Add(e => handler((TEvent)e));
    }

    /// <summary>
    /// Unsubscribe from a specific event type
    /// </summary>
    public void Off<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
        {
            // Find and remove the matching handler
            eventHandler.RemoveAll(h => h.Target == handler.Target && h.Method == handler.Method);
        }
    }

    /// <summary>
    /// Raise a combat event, notifying all subscribed handlers
    /// </summary>
    public void Raise<TEvent>(TEvent evt) where TEvent : CombatEvent
    {
        var eventType = typeof(TEvent);
        if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
        {
            foreach (var handler in eventHandler.ToList())
            {
                handler(evt);
            }
        }
    }

    /// <summary>
    /// Add a combat action to the action log
    /// </summary>
    public void AddAction(ICombatAction action)
    {
        _actions.Add(action);
    }

    /// <summary>
    /// Clear all registered listeners and handlers
    /// </summary>
    public void Clear()
    {
        foreach (var listener in _listeners.ToList())
        {
            UnregisterListener(listener);
        }
        _listeners.Clear();
        _eventHandlers.Clear();
    }

    /// <summary>
    /// Deals ability damage from <paramref name="source"/> to <paramref name="target"/>.
    /// Runs the full damage pipeline (e.g. crit, armor mitigation), applies HP mutation,
    /// records a <see cref="DamageAction"/>, and raises post-damage events.
    /// <para>
    /// Pass <paramref name="onHitStatus"/> to queue a status effect scaled to the final damage dealt
    /// (e.g. burn = 50% of final damage). The factory receives the resolved damage value and its
    /// returned status is applied through the normal <see cref="CombatPhase.StatusApplication"/> phase.
    /// </para>
    /// </summary>
    public void DealDamage(Unit source, Unit target, int baseDamage, Func<int, IStatusEffect> onHitStatus = null)
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

        AddAction(new DamageAction(source, target, ctx.FinalDamage, hpBefore, hpAfter, maxHP));

        Raise(new OnHitEvent(source, target, ctx.FinalDamage));

        ExecutePhase(CombatPhase.Healing, ctx);
        ExecutePhase(CombatPhase.ResourceGain, ctx);
        ApplyResourceGain(ctx);
        ExecutePhase(CombatPhase.StatusApplication, ctx);
        ApplyStatuses(ctx);
        ExecutePhase(CombatPhase.PostResolve, ctx);
        ApplyHealing(ctx);

        if (target.IsDead && _actions.OfType<DeathAction>().All(a => a.Target != target))
            AddAction(new DeathAction(target));
    }

    /// <summary>
    /// Resolve a full attack through all combat phases. This is the single point where
    /// HP is mutated and healing/statuses are applied.
    /// When <paramref name="effectId"/> is provided, a <see cref="StatusEffectAction"/> is
    /// recorded instead of a <see cref="DamageAction"/> (used for status effect ticks).
    /// </summary>
    public void ResolveDamage(Unit source, Unit target, int baseDamage, string effectId = null)
    {
        var ctx = new DamageContext(source, target, baseDamage);

        ExecutePhase(CombatPhase.PreAction, ctx);
        if (ctx.Cancelled) return;

        // Run existing pipeline modifiers (e.g. Rage, Crit) during DamageCalculation
        DamagePipeline.Process(ctx);
        ctx.ModifiedDamage = ctx.FinalValue;

        ExecutePhase(CombatPhase.DamageCalculation, ctx);
        ExecutePhase(CombatPhase.Mitigation, ctx);

        // Lock in the final damage value — single point of HP mutation
        ctx.FinalDamage = ctx.ModifiedDamage;

        var hpBefore = target.Stats.CurrentHP;
        var maxHP = target.Stats.MaxHP;
        target.ApplyDamage(source, ctx.FinalDamage);
        var hpAfter = target.Stats.CurrentHP;

        AddAction(effectId != null
            ? new StatusEffectAction(target, effectId, ctx.FinalDamage, hpBefore, hpAfter, maxHP)
            : new DamageAction(source, target, ctx.FinalDamage, hpBefore, hpAfter, maxHP));

        Raise(new OnHitEvent(source, target, ctx.FinalDamage));

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

        // Apply healing after PostResolve so listeners in PostResolve (e.g. Lifesteal) can queue healing first
        ApplyHealing(ctx);

        if (target.IsDead && _actions.OfType<DeathAction>().All(a => a.Target != target))
            AddAction(new DeathAction(target));
    }

    private void ExecutePhase(CombatPhase phase, DamageContext context)
    {
        Raise(new DamagePhaseEvent(phase, context));
    }

    private void ApplyHealing(DamageContext context)
    {
        if (context.PendingHealing <= 0) return;

        var hpBefore = context.Source.Stats.CurrentHP;
        context.Source.Heal(context.PendingHealing);
        var hpAfter = context.Source.Stats.CurrentHP;

        AddAction(new HealAction(context.Source, context.PendingHealing, hpBefore, hpAfter, context.Source.Stats.MaxHP));
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
