using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Instance-based combat engine that uses event-driven architecture.
/// Replaces static CombatSystem with extensible pattern.
/// Not thread-safe: a single instance must not run concurrent fights.
/// </summary>
public class CombatEngine
{
    private readonly CombatContext _context = new();

    private Unit _attacker;
    private Unit _defender;
    private bool _attackerTurn;
    private int _round;

    public List<ICombatAction> RunFight(Unit attacker, Unit defender)
    {
        Initialize(attacker, defender);

        try
        {
            while (!IsFinished())
                ExecuteRound();

            return BuildResult();
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "CombatEngine.RunFight failed", new
            {
                attacker = _attacker.Name,
                defender = _defender.Name,
                round = _round,
                attackerHp = _attacker.Stats.CurrentHP,
                defenderHp = _defender.Stats.CurrentHP,
                attackerTurn = _attackerTurn
            });

            throw;
        }
        finally
        {
            // Clean up listeners
            _context.Clear();
        }
    }

    private void Initialize(Unit attacker, Unit defender)
    {
        _attacker = attacker;
        _defender = defender;
        _round = 0;

        _context.Clear();

        // Register armor mitigation as a global combat rule (applied in Mitigation phase)
        _context.RegisterListener(new ArmorMitigationModifier());

        // Register combat listeners from both units
        RegisterListeners(attacker);
        RegisterListeners(defender);

        _attackerTurn = attacker.Stats.Speed >= defender.Stats.Speed;
    }

    private bool IsFinished()
    {
        // HP-based check is used between rounds so that units starting at 0 HP are handled correctly.
        // isDead-based checks inside ExecuteRound serve as within-round early exits when a unit
        // dies from a status effect or ability before the normal attack phase.
        return _attacker.Stats.CurrentHP <= 0 || _defender.Stats.CurrentHP <= 0;
    }

    private void ExecuteRound()
    {
        _round++;

        var acting = _attackerTurn ? _attacker : _defender;
        var target = _attackerTurn ? _defender : _attacker;

        TickStatusesTurnStart(acting, target);
        if (acting.IsDead || target.IsDead)
            return;

        TriggerAbilities(acting, target);

        if (acting.IsDead || target.IsDead)
            return;

        Attack(acting, target);
        if (acting.IsDead || target.IsDead)
            return;
        
        TickStatusesTurnEnd(acting, target);

        _attackerTurn = !_attackerTurn;
    }

    private List<ICombatAction> BuildResult()
    {
        return _context.Actions.ToList();
    }

    private void RegisterListeners(Unit unit)
    {
        // Register passives that implement ICombatListener
        foreach (var passive in unit.Passives)
        {
            if (passive is ICombatListener listener)
            {
                _context.RegisterListener(listener);
            }
        }

        // Register abilities that implement ICombatListener
        foreach (var ability in unit.Abilities)
        {
            if (ability is ICombatListener listener)
            {
                _context.RegisterListener(listener);
            }
        }
    }

    private void Attack(Unit source, Unit target)
    {
        // Raise before attack event for pre-resolution listeners
        _context.Raise(new BeforeAttackEvent(source, target));

        // Pass raw AttackPower — armor reduction is applied in the Mitigation phase by ArmorMitigationModifier
        _context.ResolveDamage(source, target, source.Stats.AttackPower);

        // Raise AfterAttackEvent after full resolution so post-resolution effects (e.g. DoubleStrike) can react
        _context.Raise(new AfterAttackEvent(source, target));
    }

    private void TickStatusesTurnStart(Unit source, Unit target)
    {
        if (!source.StatusEffects.Any())
            return;

        for (var i = source.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = source.StatusEffects[i];

            var damage = effect.OnTurnStart(source);

            if (damage > 0)
                _context.ResolveDamage(null, source, damage, effect.Id);

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(source);
                source.StatusEffects.RemoveAt(i);
            }

            if (source.IsDead) 
                return;
        }
    }

    private void TriggerAbilities(Unit source, Unit target)
    {
        if (!source.Abilities.Any())
            return;

        foreach (var ability in source.Abilities)
        {
            ability.OnCast(source, target, _context);
        }
    }

    private void TickStatusesTurnEnd(Unit source, Unit target)
    {
        if (!source.StatusEffects.Any())
            return;

        for (var i = source.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = source.StatusEffects[i];

            var damage = effect.OnTurnEnd(source);

            if (damage > 0)
                _context.ResolveDamage(null, source, damage, effect.Id);

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(source);
                source.StatusEffects.RemoveAt(i);
            }

            if (source.IsDead) 
                return;
        }
    }
}
