using System;

/// <summary>
/// Base class for all combat events
/// </summary>
public abstract class CombatEvent
{
    public Unit Source { get; }
    public Unit Target { get; }

    /// <summary>
    /// Combat phase associated with this event. Set on phase-specific events such as <see cref="DamagePhaseEvent"/>.
    /// </summary>
    public CombatPhase Phase { get; }

    protected CombatEvent(Unit source, Unit target)
    {
        Source = source;
        Target = target;
    }

    protected CombatEvent(CombatPhase phase)
    {
        Phase = phase;
    }

    protected CombatEvent(Unit source, Unit target, CombatPhase phase)
    {
        Source = source;
        Target = target;
        Phase = phase;
    }
}

/// <summary>
/// Raised for each phase of a <see cref="CombatContext.ResolveAttack"/> call.
/// Listeners filter by <see cref="CombatEvent.Phase"/> and mutate <see cref="Context"/> instead of units directly.
/// <see cref="CombatEvent.Source"/> and <see cref="CombatEvent.Target"/> mirror <see cref="Context"/>.Source/Target.
/// </summary>
public class DamagePhaseEvent : CombatEvent
{
    public DamageContext Context { get; }

    public DamagePhaseEvent(CombatPhase phase, DamageContext context)
        : base(context.Source, context.Target, phase)
    {
        Context = context;
    }
}

/// <summary>
/// Fired before attack damage is calculated
/// </summary>
public class BeforeAttackEvent : CombatEvent
{
    public BeforeAttackEvent(Unit source, Unit target) : base(source, target) { }
}

/// <summary>
/// Fired after damage is calculated but before it's applied
/// </summary>
public class OnDamageCalculatedEvent : CombatEvent
{
    public int Damage { get; set; }
    public bool IsCritical { get; set; }

    public OnDamageCalculatedEvent(Unit source, Unit target, int damage, bool isCritical) 
        : base(source, target)
    {
        Damage = damage;
        IsCritical = isCritical;
    }
}

/// <summary>
/// Fired after damage is applied to target
/// </summary>
public class OnHitEvent : CombatEvent
{
    public int Damage { get; }

    public OnHitEvent(Unit source, Unit target, int damage) : base(source, target)
    {
        Damage = damage;
    }
}

/// <summary>
/// Fired after an attack completes (including all follow-up hits)
/// </summary>
public class AfterAttackEvent : CombatEvent
{
    public AfterAttackEvent(Unit source, Unit target) : base(source, target) { }
}

/// <summary>
/// Fired when an ability is triggered
/// </summary>
public class OnAbilityTriggerEvent : CombatEvent
{
    public IAbility Ability { get; }

    public OnAbilityTriggerEvent(Unit source, Unit target, IAbility ability) 
        : base(source, target)
    {
        Ability = ability;
    }
}
