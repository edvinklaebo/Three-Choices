using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CombatSystem
{
    public static List<ICombatAction> RunFight(Unit attacker, Unit defender)
    {
        var actions = new List<ICombatAction>();

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
                actions.AddRange(statusActions);

                if (acting.isDead)
                    break;

                List<ICombatAction> attackActions;
                if (attackerTurn)
                    attackActions = Attack(attacker, defender, round);
                else
                    attackActions = Attack(defender, attacker, round);

                actions.AddRange(attackActions);

                var endStatusActions = TickStatusesTurnEnd(acting);
                actions.AddRange(endStatusActions);

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
            Log.Exception(ex, "CombatSystem.RunFight failed", new
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

        return actions;
    }

    private static List<ICombatAction> Attack(Unit attacker, Unit defender, int round)
    {
        var actions = new List<ICombatAction>();

        Log.Info("Attack start", new
        {
            round,
            attacker = attacker.Name,
            defender = defender.Name,
            attackerHp = attacker.Stats.CurrentHP,
            defenderHp = defender.Stats.CurrentHP
        });

        var armorMultiplier = GetDamageMultiplier(defender.Stats.Armor);
        var baseDamage = Mathf.CeilToInt(attacker.Stats.AttackPower * armorMultiplier);

        var ctx = new DamageContext(attacker, defender, baseDamage);

        DamagePipeline.Process(ctx);

        Log.Info("Damage calculated", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            attackPower = attacker.Stats.AttackPower,
            defenderArmor = defender.Stats.Armor,
            armorMultiplier,
            baseDamage
        });

        // Capture HP before applying damage
        var hpBefore = defender.Stats.CurrentHP;
        var maxHP = defender.Stats.MaxHP;

        // Apply damage to unit state
        defender.ApplyDamage(attacker, ctx.FinalValue);

        // Capture HP after applying damage
        var hpAfter = defender.Stats.CurrentHP;

        Log.Info("Damage applied", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            ctx.FinalValue,
            hpBefore,
            hpAfter
        });

        // Create damage action for animation with HP values
        actions.Add(new DamageAction(attacker, defender, ctx.FinalValue, hpBefore, hpAfter, maxHP));

        // Add death action if defender died
        if (defender.isDead)
        {
            actions.Add(new DeathAction(defender));
        }

        return actions;
    }

    private static List<ICombatAction> TickStatusesTurnStart(Unit unit)
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
            if (unit.isDead)
            {
                actions.Add(new DeathAction(unit));
            }

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(unit);
                unit.StatusEffects.RemoveAt(i);
            }
        }

        return actions;
    }

    private static List<ICombatAction> TickStatusesTurnEnd(Unit unit)
    {
        var actions = new List<ICombatAction>();

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
            if (unit.isDead)
            {
                actions.Add(new DeathAction(unit));
            }

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
}