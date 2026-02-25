using System;
using System.Collections.Generic;

/// <summary>
/// Combat service container and event hub for a single combat instance.
/// Delegates event dispatch to <see cref="CombatEventBus"/>, listener lifecycle to
/// <see cref="CombatListenerRegistry"/>, action recording to <see cref="CombatActionLog"/>,
/// and damage resolution to <see cref="CombatDamageResolver"/>.
/// </summary>
public class CombatContext
{
    private readonly CombatEventBus _eventBus = new();
    private readonly CombatActionLog _actionLog = new();
    private readonly CombatListenerRegistry _listenerRegistry;
    private readonly CombatDamageResolver _damageResolver;

    public IReadOnlyList<ICombatAction> Actions => _actionLog.Actions;

    public CombatContext()
    {
        _listenerRegistry = new CombatListenerRegistry(this);
        _damageResolver = new CombatDamageResolver(_eventBus, _actionLog);
    }

    /// <summary>
    /// Register a listener for combat events.
    /// </summary>
    public void RegisterListener(ICombatListener listener) => _listenerRegistry.Register(listener);

    /// <summary>
    /// Subscribe to a specific event type.
    /// </summary>
    public void On<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent => _eventBus.On(handler);

    /// <summary>
    /// Unsubscribe from a specific event type.
    /// </summary>
    public void Off<TEvent>(Action<TEvent> handler) where TEvent : CombatEvent => _eventBus.Off(handler);

    /// <summary>
    /// Raise a combat event, notifying all subscribed handlers.
    /// </summary>
    public void Raise<TEvent>(TEvent evt) where TEvent : CombatEvent => _eventBus.Raise(evt);

    /// <summary>
    /// Add a combat action to the action log.
    /// </summary>
    public void AddAction(ICombatAction action) => _actionLog.Add(action);

    /// <summary>
    /// Clear all registered listeners and handlers.
    /// </summary>
    public void Clear()
    {
        _listenerRegistry.Clear();
        _eventBus.Clear();
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
        => _damageResolver.DealDamage(source, target, baseDamage, onHitStatus, effectId);
}
