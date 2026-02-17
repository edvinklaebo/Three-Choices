using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class LifestealTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = 0,
                    Speed = speed
                }
            };
        }

        [Test]
        public void Lifesteal_HealsAttacker_WhenDamageIsDealt()
        {
            var attacker = CreateUnit("Vampire", 50, 10, 10);
            attacker.Stats.CurrentHP = 30; // Reduce HP to test healing
            var defender = CreateUnit("Victim", 100, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            var initialHP = attacker.Stats.CurrentHP;

            // Run combat
            CombatSystem.RunFight(attacker, defender);

            // Attacker should have healed from lifesteal
            // With 10 attack and 20% lifesteal, should heal 2 HP per hit
            Assert.Greater(attacker.Stats.CurrentHP, initialHP, "Lifesteal should heal the attacker");
        }

        [Test]
        public void Lifesteal_QueuesHealAction_InCombat()
        {
            var attacker = CreateUnit("Vampire", 100, 20, 10);
            attacker.Stats.CurrentHP = 80; // Reduce HP so healing is visible
            var defender = CreateUnit("Victim", 100, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            // Run combat to get actions
            var actions = CombatSystem.RunFight(attacker, defender);

            // Should have HealAction in the action list
            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Combat should queue HealAction for lifesteal");

            // Each heal action should have valid data
            foreach (var healAction in healActions)
            {
                Assert.AreEqual(attacker, healAction.Target, "Heal target should be the attacker");
                Assert.Greater(healAction.Amount, 0, "Heal amount should be positive");
                Assert.Greater(healAction.TargetHPAfter, healAction.TargetHPBefore, 
                    "HP after should be greater than HP before for healing");
                Assert.Greater(healAction.TargetMaxHP, 0, "Max HP should be set");
            }
        }

        [Test]
        public void Lifesteal_HealsCorrectAmount_BasedOnDamage()
        {
            var attacker = CreateUnit("Vampire", 100, 50, 10);
            attacker.Stats.CurrentHP = 50;
            var defender = CreateUnit("Victim", 100, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            var initialHP = attacker.Stats.CurrentHP;

            // Run combat - attacker deals 50 damage per hit
            // 20% of 50 = 10 HP healed per hit
            CombatSystem.RunFight(attacker, defender);

            // Should heal 10 HP per attack (20% of 50 damage)
            // Defender has 100 HP, so attacker hits twice: 2 * 10 = 20 HP healed
            var expectedHP = Mathf.Min(100, initialHP + 20);
            Assert.AreEqual(expectedHP, attacker.Stats.CurrentHP, 
                "Should heal 20% of damage dealt (10 HP per 50 damage hit)");
        }

        [Test]
        public void Lifesteal_HealsCorrectAmount_WhenExactPercentage()
        {
            var attacker = CreateUnit("Vampire", 100, 10, 10);
            attacker.Stats.CurrentHP = 90;
            var defender = CreateUnit("Victim", 15, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            // Run combat - attacker deals 10 damage per hit
            // 20% of 10 = 2 HP healed per hit (no rounding needed)
            var actions = CombatSystem.RunFight(attacker, defender);

            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Should have heal actions");

            // Each heal should be 2 HP (10 * 0.2 = 2)
            foreach (var healAction in healActions)
            {
                Assert.AreEqual(2, healAction.Amount, "Should heal 2 HP per 10 damage");
            }
        }

        [Test]
        public void Lifesteal_RoundsUpHealAmount()
        {
            var attacker = CreateUnit("Vampire", 100, 7, 10);
            attacker.Stats.CurrentHP = 90;
            var defender = CreateUnit("Victim", 30, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            // Run combat - attacker deals 7 damage per hit
            // 20% of 7 = 1.4, which rounds up to 2 HP healed per hit
            var actions = CombatSystem.RunFight(attacker, defender);

            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Should have heal actions");

            // Each heal should be 2 HP (ceiling of 7 * 0.2 = 1.4)
            foreach (var healAction in healActions)
            {
                Assert.AreEqual(2, healAction.Amount, "Should round up: 7 * 0.2 = 1.4 -> 2 HP");
            }
        }

        [Test]
        public void Lifesteal_DoesNotHealAboveMaxHP()
        {
            var attacker = CreateUnit("Vampire", 100, 50, 10);
            attacker.Stats.CurrentHP = 99; // Almost full
            var defender = CreateUnit("Victim", 100, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            // Run combat
            CombatSystem.RunFight(attacker, defender);

            // Should not exceed max HP
            Assert.LessOrEqual(attacker.Stats.CurrentHP, attacker.Stats.MaxHP, 
                "Lifesteal should not heal above max HP");
            Assert.AreEqual(100, attacker.Stats.CurrentHP, 
                "Should be at max HP after healing");
        }

        [Test]
        public void Lifesteal_TracksHPValuesCorrectly()
        {
            var attacker = CreateUnit("Vampire", 100, 10, 10);
            attacker.Stats.CurrentHP = 80;
            var defender = CreateUnit("Victim", 20, 0, 5);

            // Add lifesteal passive (20% healing)
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(lifesteal);

            // Run combat
            var actions = CombatSystem.RunFight(attacker, defender);

            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Should have heal actions");

            // Verify HP tracking for first heal
            var firstHeal = healActions[0];
            Assert.AreEqual(80, firstHeal.TargetHPBefore, 
                "First heal should start from 80 HP");
            Assert.AreEqual(82, firstHeal.TargetHPAfter, 
                "First heal should go to 82 HP (80 + 2)");
            Assert.AreEqual(100, firstHeal.TargetMaxHP, "Max HP should be 100");
        }
    }
}
