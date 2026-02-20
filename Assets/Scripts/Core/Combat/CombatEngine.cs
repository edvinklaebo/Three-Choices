using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Instance-based combat engine that uses event-driven architecture
/// Replaces static CombatSystem with extensible pattern
/// </summary>
public class CombatEngine
{
    private readonly CombatContext _context = new();

    public List<ICombatAction> RunFight(Unit attacker, Unit defender)
    {
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
            defenerSpeed = defender.Stats.Speed
        });

        var attackerTurn = attacker.Stats.Speed >= defender.Stats.Speed;

        Log.Info("Turn order decided", new
        {
            first = attackerTurn ? attacker.Name : defender.Name,
            reason = "speed",
            aSpeed = attacker.Stats.Speed,
            bSpeed = defender.Stats.Speed
        });

        var round = 0;

        try
        {
            while (attacker.Stats.CurrentHP > 0 && defender.Stats.CurrentHP > 0)
            {
                round++;

                Log.Info("Combat round start", new
                {
                    round,
                    aHp = attacker.Stats.CurrentHP,
                    bHp = defender.Stats.CurrentHP,
                    turn = attackerTurn ? attacker.Name : defender.Name
                });

                var acting = attackerTurn ? attacker : defender;

                var statusActions = TickStatusesTurnStart(acting);
                foreach (var action in statusActions)
                    _context.AddAction(action);

                if (acting.isDead)
                    break;

                // Trigger abilities at turn start (e.g., Fireball)
                var target = attackerTurn ? defender : attacker;
                var abilityActions = TriggerAbilities(acting, target);
                foreach (var action in abilityActions)
                    _context.AddAction(action);

                if (target.isDead)
                    break;

                // Execute attack with event-driven flow
                Attack(attackerTurn ? attacker : defender, attackerTurn ? defender : attacker, round);

                var endStatusActions = TickStatusesTurnEnd(acting);
                foreach (var action in endStatusActions)
                    _context.AddAction(action);

                attackerTurn = !attackerTurn;
            }

            var winner = attacker.Stats.CurrentHP > 0 ? attacker.Name : defender.Name;

            Log.Info("Combat finished", new
            {
                winner,
                rounds = round,
                finalAHp = attacker.Stats.CurrentHP,
                finalBHp = defender.Stats.CurrentHP
            });
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "CombatEngine.RunFight failed", new
            {
                attacker = attacker.Name,
                defender = defender.Name,
                round,
                attackerHp = attacker.Stats.CurrentHP,
                defenderHp = defender.Stats.CurrentHP,
                attackerTurn
            });

            throw;
        }
        finally
        {
            // Clean up listeners
            _context.Clear();
        }

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
