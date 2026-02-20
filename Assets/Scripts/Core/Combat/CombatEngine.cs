using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        // Register combat listeners from both units
        RegisterListeners(attacker);
        RegisterListeners(defender);

        Log.Info("Combat started", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            attackerHp = attacker.Stats.CurrentHP,
            defenderHp = defender.Stats.CurrentHP,
            attackerAtk = attacker.Stats.AttackPower,
            defenderAtk = defender.Stats.AttackPower,
            attackerArmor = attacker.Stats.Armor,
            defenderArmor = defender.Stats.Armor,
            attackerSpeed = attacker.Stats.Speed,
            defenderSpeed = defender.Stats.Speed
        });

        _attackerTurn = attacker.Stats.Speed >= defender.Stats.Speed;

        Log.Info("Turn order decided", new
        {
            first = _attackerTurn ? attacker.Name : defender.Name,
            reason = "speed",
            aSpeed = attacker.Stats.Speed,
            bSpeed = defender.Stats.Speed
        });
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

        Log.Info("Combat round start", new
        {
            round = _round,
            aHp = _attacker.Stats.CurrentHP,
            bHp = _defender.Stats.CurrentHP,
            turn = _attackerTurn ? _attacker.Name : _defender.Name
        });

        var acting = _attackerTurn ? _attacker : _defender;
        var target = _attackerTurn ? _defender : _attacker;

        var statusActions = TickStatusesTurnStart(acting);
        foreach (var action in statusActions)
            _context.AddAction(action);

        if (acting.isDead)
            return;

        // Trigger abilities at turn start (e.g., Fireball)
        var abilityActions = TriggerAbilities(acting, target);
        foreach (var action in abilityActions)
            _context.AddAction(action);

        if (target.isDead)
            return;

        // Execute attack with event-driven flow
        Attack(acting, target, _round);

        var endStatusActions = TickStatusesTurnEnd(acting);
        foreach (var action in endStatusActions)
            _context.AddAction(action);

        _attackerTurn = !_attackerTurn;
    }

    private List<ICombatAction> BuildResult()
    {
        var winner = _attacker.Stats.CurrentHP > 0 ? _attacker.Name : _defender.Name;

        Log.Info("Combat finished", new
        {
            winner,
            rounds = _round,
            finalAHp = _attacker.Stats.CurrentHP,
            finalBHp = _defender.Stats.CurrentHP
        });

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

    private void Attack(Unit attacker, Unit defender, int round)
    {
        Log.Info("Attack start", new
        {
            round,
            attacker = attacker.Name,
            defender = defender.Name,
            attackerHp = attacker.Stats.CurrentHP,
            defenderHp = defender.Stats.CurrentHP
        });

        // Raise before attack event for pre-resolution listeners
        _context.Raise(new BeforeAttackEvent(attacker, defender));

        var armorMultiplier = GetDamageMultiplier(defender.Stats.Armor);
        var baseDamage = Mathf.CeilToInt(attacker.Stats.AttackPower * armorMultiplier);

        Log.Info("Damage calculated", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            attackPower = attacker.Stats.AttackPower,
            defenderArmor = defender.Stats.Armor,
            armorMultiplier,
            baseDamage
        });

        // Resolve all phases: DamageCalculation, Mitigation, DamageApplication, Healing, etc.
        _context.ResolveAttack(attacker, defender, baseDamage);

        // Raise AfterAttackEvent after full resolution so post-resolution effects (e.g. DoubleStrike) can react
        _context.Raise(new AfterAttackEvent(attacker, defender));
    }

    private List<ICombatAction> TickStatusesTurnStart(Unit unit)
    {
        var actions = new List<ICombatAction>();

        if (!unit.StatusEffects.Any())
            return actions;

        for (var i = unit.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = unit.StatusEffects[i];

            // Track HP before status tick
            var hpBefore = unit.Stats.CurrentHP;
            var maxHP = unit.Stats.MaxHP;

            effect.OnTurnStart(unit);

            // If damage was dealt, create an action
            var hpAfter = unit.Stats.CurrentHP;
            if (hpAfter < hpBefore)
            {
                var damage = hpBefore - hpAfter;
                actions.Add(new StatusEffectAction(unit, effect.Id, damage, hpBefore, hpAfter, maxHP));
            }

            // Check for death after status effect
            if (unit.isDead) actions.Add(new DeathAction(unit));

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(unit);
                unit.StatusEffects.RemoveAt(i);
            }
        }

        return actions;
    }

    private List<ICombatAction> TriggerAbilities(Unit source, Unit target)
    {
        var actions = new List<ICombatAction>();

        if (!source.Abilities.Any())
            return actions;

        foreach (var ability in source.Abilities)
        {
            Log.Info("Triggering ability", new
            {
                source = source.Name,
                target = target.Name,
                ability = ability.GetType().Name
            });

            // Capture HP before ability
            var hpBefore = target.Stats.CurrentHP;

            // Trigger the ability (applies damage)
            ability.OnAttack(source, target);

            // Capture HP after ability
            var hpAfter = target.Stats.CurrentHP;

            // Let ability create its own actions if it implements IActionCreator
            if (ability is IActionCreator actionCreator)
            {
                actionCreator.CreateActions(_context, source, target, hpBefore, hpAfter);
            }

            // Check for death after ability
            if (target.isDead)
            {
                if (!_context.Actions.OfType<DeathAction>().Any(a => a.Target == target))
                    _context.AddAction(new DeathAction(target));
            }
        }

        return actions;
    }

    private List<ICombatAction> TickStatusesTurnEnd(Unit unit)
    {
        var actions = new List<ICombatAction>();

        if (!unit.StatusEffects.Any())
            return actions;

        for (var i = unit.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = unit.StatusEffects[i];

            // Track HP before status tick
            var hpBefore = unit.Stats.CurrentHP;
            var maxHP = unit.Stats.MaxHP;

            effect.OnTurnEnd(unit);

            // If damage was dealt, create an action
            var hpAfter = unit.Stats.CurrentHP;
            if (hpAfter < hpBefore)
            {
                var damage = hpBefore - hpAfter;
                actions.Add(new StatusEffectAction(unit, effect.Id, damage, hpBefore, hpAfter, maxHP));
            }

            // Check for death after status effect
            if (unit.isDead) actions.Add(new DeathAction(unit));

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(unit);
                unit.StatusEffects.RemoveAt(i);
            }
        }

        return actions;
    }

    private static float GetDamageMultiplier(int armor)
    {
        var multiplier = 100f / (100f + armor);

        Log.Info("Armor multiplier computed", new
        {
            armor,
            multiplier
        });

        return multiplier;
    }

    public CombatContext Context => _context;
}
