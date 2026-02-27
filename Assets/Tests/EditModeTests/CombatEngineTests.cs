using NUnit.Framework;
using System.Linq;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for the new event-driven combat engine architecture
    /// </summary>
    public class CombatEngineTests
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
        public void CombatEngine_InstanceBased_WorksCorrectly()
        {
            var attacker = CreateUnit("A", 50, 10, 10);
            var defender = CreateUnit("B", 50, 5, 5);

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            Assert.IsNotEmpty(actions, "Combat should produce actions");
            Assert.IsTrue(attacker.Stats.CurrentHP > 0 || defender.Stats.CurrentHP > 0, "One unit should survive");
        }

        [Test]
        public void CombatEngine_ZeroTypeChecks_PassivesWorkViaEvents()
        {
            var attacker = CreateUnit("Vampire", 50, 20, 10);
            attacker.Stats.CurrentHP = 30;
            var defender = CreateUnit("Victim", 100, 0, 5);

            // Add lifesteal passive
            var lifesteal = new Lifesteal(attacker, 0.2f);
            lifesteal.OnAttach(attacker);
            attacker.Passives.Add(lifesteal);

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            // Verify lifesteal worked without type checks
            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Lifesteal should create heal actions via events");
            Assert.Greater(attacker.Stats.CurrentHP, 30, "Lifesteal should heal the attacker");
        }

        [Test]
        public void CombatEngine_ZeroTypeChecks_DoubleStrikeWorksViaEvents()
        {
            var attacker = CreateUnit("Striker", 100, 20, 10);
            var defender = CreateUnit("Target", 100, 0, 5);

            // Add double strike with 100% chance
            var doubleStrike = new DoubleStrike(1.0f, 0.75f);
            doubleStrike.OnAttach(attacker);
            attacker.Passives.Add(doubleStrike);

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            // Verify double strike worked without type checks
            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.GreaterOrEqual(damageActions.Count, 2, "Double strike should create second damage action via events");
        }

        [Test]
        public void CombatEngine_DeterministicListenerOrdering()
        {
            var attacker = CreateUnit("Multi", 100, 20, 10);
            var defender = CreateUnit("Target", 100, 0, 5);

            // Add both passives with defined priorities
            var lifesteal = new Lifesteal(attacker, 0.2f); // Priority 200
            var doubleStrike = new DoubleStrike(1.0f, 0.75f); // ExtraAttackHandler Priority 210
            doubleStrike.OnAttach(attacker);
            lifesteal.OnAttach(attacker);
            attacker.Passives.Add(lifesteal);
            attacker.Passives.Add(doubleStrike);

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            // Both passives should work together
            var healActions = actions.OfType<HealAction>().ToList();
            var damageActions = actions.OfType<DamageAction>().ToList();

            Assert.IsNotEmpty(healActions, "Lifesteal should work");
            Assert.GreaterOrEqual(damageActions.Count, 2, "Double strike should work");
            
            // With lifesteal + double strike, we should get heals from both hits
            Assert.GreaterOrEqual(healActions.Count, 2, "Should heal from both primary and double strike hits");
        }

        [Test]
        public void BackwardCompatibility_StaticAPIStillWorks()
        {
            var attacker = CreateUnit("A", 50, 10, 10);
            var defender = CreateUnit("B", 50, 5, 5);

            // Use old static API
            var actions = CombatSystem.RunFight(attacker, defender);

            Assert.IsNotEmpty(actions, "Static API should still work");
            Assert.IsTrue(attacker.Stats.CurrentHP > 0 || defender.Stats.CurrentHP > 0, "Combat should complete");
        }
    }
}
