using System;

/// <summary>
/// Deals damage equal to half the owner's armour to any unit that attacks them.
/// Reflects only on direct attacks (attacker != null) to avoid triggering on status effects.
/// Uses a re-entrance guard to prevent infinite reflect chains when both units carry Thorns.
/// When registered as an <see cref="ICombatListener"/> (inside <see cref="CombatEngine"/>),
/// reflects via <see cref="CombatContext.DealDamage"/> so a <see cref="DamageAction"/> is
/// created and the damage is displayed to the player.
/// </summary>
[Serializable]
public class Thorns : IPassive, ICombatListener, IActionCreator
{
    [NonSerialized] private Unit _owner;
    [NonSerialized] private CombatContext _context;
    [NonSerialized] private bool _isReflecting;

    public int Priority => 100;

    public void OnAttach(Unit owner)
    {
        _owner = owner;
    }

    public void OnDetach(Unit owner)
    {
        _owner = null;
    }

    public void RegisterHandlers(CombatContext context)
    {
        _context = context;
        context.On<OnHitEvent>(OnHit);
    }

    public void UnregisterHandlers(CombatContext context)
    {
        context.Off<OnHitEvent>(OnHit);
        _context = null;
    }

    // Called via the combat event bus when a hit lands. Routes thorn reflect through
    // DealDamage so a DamageAction is recorded and the damage is displayed.
    private void OnHit(OnHitEvent evt)
    {
        if (_owner == null)
            return;

        if (evt.Target != _owner)
            return;

        if (evt.Source == null)
            return;

        if (_isReflecting)
            return;

        var thornsDamage = _owner.Stats.Armor / 2;
        if (thornsDamage <= 0)
            return;

        Log.Info($"Thorns reflect: {_owner.Name} dealt {thornsDamage} to {evt.Source.Name}");

        _isReflecting = true;
        try
        {
            _context.DealDamage(_owner, evt.Source, thornsDamage, actionCreator: this);
        }
        finally
        {
            _isReflecting = false;
        }
    }

    public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
        => new ThornsAction(source, target, finalDamage, hpBefore, hpAfter, maxHP);
    
}