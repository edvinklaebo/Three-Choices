using System;

/// <summary>
/// Base class for all combat events
/// </summary>
public abstract class CombatEvent
{
    public Unit Source { get; }
    public Unit Target { get; }

    protected CombatEvent(Unit source, Unit target)
    {
        Source = source;
        Target = target;
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
