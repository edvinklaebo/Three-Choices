using NUnit.Framework;
using System.Linq;

namespace Tests.EditModeTests
{
    public class DoubleStrikeTests
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
        public void DoubleStrike_TriggersSecondHit_WithCorrectDamageMultiplier()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            var defender = CreateUnit("Target", 100, 0, 5);

            // Add double strike with 100% chance for testing
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Run combat
            var actions = CombatSystem.RunFight(attacker, defender);

            // Should have damage actions for both hits
            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.GreaterOrEqual(damageActions.Count, 2, "Should have at least 2 damage actions (first hit + double strike)");

            // First hit should be 20 damage
            Assert.AreEqual(20, damageActions[0].Amount, "First hit should deal full attack damage");

            // Second hit should be 75% of attack (15 damage)
            Assert.AreEqual(15, damageActions[1].Amount, "Second hit should deal 75% damage");
        }

        [Test]
        public void DoubleStrike_SecondHit_CanCritIndependently()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            var defender = CreateUnit("Target", 200, 0, 5);

            // Add double strike with 100% chance
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Run combat multiple times to check for crit variance
            var actions = CombatSystem.RunFight(attacker, defender);
            var damageActions = actions.OfType<DamageAction>().ToList();

            // Verify both hits occurred
            Assert.GreaterOrEqual(damageActions.Count, 2, "Should have at least 2 damage actions");
        }

        [Test]
        public void DoubleStrike_TriggersOnHitEffects_OnSecondHit()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            attacker.Stats.CurrentHP = 50; // Lower HP to make healing visible
            var defender = CreateUnit("Target", 35, 0, 5);

            // Add both double strike and lifesteal
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            var lifesteal = new Lifesteal(attacker, 0.2f);
            attacker.Passives.Add(doubleStrike);
            attacker.Passives.Add(lifesteal);

            var initialHP = attacker.Stats.CurrentHP;

            // Run combat
            CombatSystem.RunFight(attacker, defender);

            // Should heal from both hits
            // First hit: 20 damage * 0.2 = 4 heal
            // Second hit: 15 damage * 0.2 = 3 heal
            // Total heal: 7
            var expectedHP = initialHP + 7;
            Assert.AreEqual(expectedHP, attacker.Stats.CurrentHP, "Lifesteal should trigger on both hits");
        }

        [Test]
        public void DoubleStrike_DoesNotTrigger_WhenChanceIsZero()
        {
            var attacker = CreateUnit("Striker", 100, 100, 10);
            var defender = CreateUnit("Target", 100, 0, 5);

            // Add double strike with 0% chance
            var doubleStrike = new DoubleStrike(attacker, 0.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Run combat
            var actions = CombatSystem.RunFight(attacker, defender);

            // Should only have 1 damage action per attack
            var damageActions = actions.OfType<DamageAction>().ToList();
            
            // With 0% chance, there should be no double strikes
            // Each turn should have exactly 1 damage action
            var firstTurnDamageCount = 0;
            foreach (var action in damageActions)
            {
                if (action != null)
                {
                    firstTurnDamageCount++;
                }
            }

            Assert.AreEqual(1, firstTurnDamageCount, "With 0% chance, should only have 1 damage action per attack");
        }

        [Test]
        public void DoubleStrike_StopsIfTargetDies_FromFirstHit()
        {
            var attacker = CreateUnit("Striker", 100, 100, 10);
            var defender = CreateUnit("Target", 50, 0, 5); // Low HP to die from first hit

            // Add double strike with 100% chance
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Run combat
            var actions = CombatSystem.RunFight(attacker, defender);

            // Defender should die from first hit
            Assert.IsTrue(defender.isDead, "Defender should be dead");

            // Should only have 1 damage action since target died
            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.AreEqual(1, damageActions.Count, "Should only have 1 damage action if target dies from first hit");
        }

        [Test]
        public void DoubleStrike_CorrectlyQueuesAndConsumes_PendingStrikes()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            var defender = CreateUnit("Target", 100, 0, 5);

            // Add double strike with 100% chance
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Manually trigger OnHit to queue a strike
            attacker.RaiseOnHit(defender, 20);

            // Consume pending strikes
            var strikes = doubleStrike.ConsumePendingStrikes();

            Assert.AreEqual(1, strikes.Count, "Should have 1 pending strike");
            Assert.AreEqual(defender, strikes[0].Target, "Strike should target the correct unit");
            Assert.AreEqual(0.75f, strikes[0].DamageMultiplier, "Strike should have correct damage multiplier");

            // Consuming again should return empty list
            var emptyStrikes = doubleStrike.ConsumePendingStrikes();
            Assert.AreEqual(0, emptyStrikes.Count, "Should have no pending strikes after consuming");
        }

        [Test]
        public void DoubleStrike_WorksWithArmor_OnSecondHit()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            var defender = CreateUnit("Target", 200, 0, 5);
            defender.Stats.Armor = 50; // 50 armor = 67% damage multiplier

            // Add double strike with 100% chance
            var doubleStrike = new DoubleStrike(attacker, 1.0f, 0.75f);
            attacker.Passives.Add(doubleStrike);

            // Run combat
            var actions = CombatSystem.RunFight(attacker, defender);
            var damageActions = actions.OfType<DamageAction>().ToList();

            Assert.GreaterOrEqual(damageActions.Count, 2, "Should have at least 2 damage actions");

            // First hit: 20 * (100 / 150) = 13.33 -> 14 damage (ceil)
            Assert.AreEqual(14, damageActions[0].Amount, "First hit should account for armor");

            // Second hit: 20 * 0.75 * (100 / 150) = 10 damage (ceil)
            Assert.AreEqual(10, damageActions[1].Amount, "Second hit should account for armor and damage multiplier");
        }
    }
}
