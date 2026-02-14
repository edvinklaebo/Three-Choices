using System;
using UnityEngine;

public static class CombatSystem
{
    public static void RunFight(Unit attacker, Unit defender)
    {
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

                acting.TickStatusesTurnStart();

                if (acting.isDead)
                    break;

                if (attackerTurn)
                    Attack(attacker, defender, round);
                else
                    Attack(defender, attacker, round);

                acting.TickStatusesTurnEnd();

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
    }

    private static void Attack(Unit attacker, Unit defender, int round)
    {
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

        defender.ApplyDamage(attacker, ctx.FinalValue);

        Log.Info("Damage applied", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            ctx.FinalValue,
            defenderHpAfter = defender.Stats.CurrentHP
        });
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