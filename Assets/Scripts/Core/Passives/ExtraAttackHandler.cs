using UnityEngine;

/// <summary>
/// Handles the combat-pipeline integration for <see cref="DoubleStrike"/>:
/// executes pending extra attacks after each attack, enforces recursion safety,
/// and emits log entries.  Created and registered by <see cref="CombatEngine"/>
/// via <see cref="ICombatHandlerProvider"/>.
/// </summary>
public class ExtraAttackHandler : ICombatListener
{
    private readonly Unit _owner;
    private readonly DoubleStrike _passive;
    private CombatContext _context;
    private bool _isProcessingStrikes;

    public int Priority => 210; // Late priority — after damage is dealt, after lifesteal

    public ExtraAttackHandler(Unit owner, DoubleStrike passive)
    {
        _owner = owner;
        _passive = passive;
    }

    public void RegisterHandlers(CombatContext context)
    {
        _context = context;
        context.On<AfterAttackEvent>(OnAfterAttack);
    }

    public void UnregisterHandlers(CombatContext context)
    {
        _context = null;
        context.Off<AfterAttackEvent>(OnAfterAttack);
    }

    private void OnAfterAttack(AfterAttackEvent evt)
    {
        if (evt.Source != _owner)
            return;

        if (_isProcessingStrikes)
            return;

        _isProcessingStrikes = true;
        _passive.Suspend();
        try
        {
            var strikes = _passive.ConsumePendingStrikes();
            foreach (var strikeData in strikes)
            {
                if (strikeData.Target.IsDead)
                    continue;

                var secondBaseDamage = Mathf.CeilToInt(_owner.Stats.AttackPower * strikeData.DamageMultiplier);

                Log.Info("Double Strike second hit", new
                {
                    attacker = _owner.Name,
                    target = strikeData.Target.Name,
                    secondBaseDamage,
                    strikeData.DamageMultiplier
                });

                _context.DealDamage(_owner, strikeData.Target, secondBaseDamage);
            }
        }
        finally
        {
            _passive.Resume();
            _isProcessingStrikes = false;
        }
    }
}
