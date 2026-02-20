using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class CombatAnimationSystemTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = armor,
                    Speed = speed
                }
            };
        }

        [Test]
        public void CombatSystem_ReturnsActions_InsteadOfVoid()
        {
            var unitA = CreateUnit("A", 10, 5, 0, 5);
            var unitB = CreateUnit("B", 10, 4, 0, 4);

            var actions = CombatSystem.RunFight(unitA, unitB);

            Assert.IsNotNull(actions, "RunFight should return a list of actions");
            Assert.IsInstanceOf<List<ICombatAction>>(actions);
        }

        [Test]
        public void CombatSystem_ProducesDamageActions()
        {
            var unitA = CreateUnit("A", 50, 10, 0, 5);
            var unitB = CreateUnit("B", 50, 10, 0, 4);

            var actions = CombatSystem.RunFight(unitA, unitB);

            var damageActions = new List<ICombatAction>();
            foreach (var action in actions)
                if (action is DamageAction)
                    damageActions.Add(action);

            Assert.Greater(damageActions.Count, 0, "Combat should produce at least one damage action");
        }

        [Test]
        public void CombatSystem_ProducesDeathAction_WhenUnitDies()
        {
            var unitA = CreateUnit("A", 100, 10, 0, 5);
            var unitB = CreateUnit("B", 10, 1, 0, 4);

            var actions = CombatSystem.RunFight(unitA, unitB);

            var deathActions = new List<DeathAction>();
            foreach (var action in actions)
                if (action is DeathAction death)
                    deathActions.Add(death);

            Assert.AreEqual(1, deathActions.Count, "Should produce exactly one death action");
            Assert.AreEqual("B", deathActions[0].Target.Name, "Dead unit should be B");
        }

        [Test]
        public void DamageAction_ContainsCorrectData()
        {
            var attacker = CreateUnit("Attacker", 100, 20, 0, 10);
            var defender = CreateUnit("Defender", 100, 10, 0, 5);

            var actions = CombatSystem.RunFight(attacker, defender);

            var firstDamage = actions[0] as DamageAction;
            Assert.IsNotNull(firstDamage, "First action should be a damage action");
            Assert.AreEqual("Attacker", firstDamage.Source.Name);
            Assert.AreEqual("Defender", firstDamage.Target.Name);
            Assert.Greater(firstDamage.Amount, 0, "Damage amount should be positive");
        }

        [Test]
        public void DeathAction_ContainsCorrectTarget()
        {
            var attacker = CreateUnit("Attacker", 100, 50, 0, 10);
            var defender = CreateUnit("Defender", 10, 1, 0, 5);

            var actions = CombatSystem.RunFight(attacker, defender);

            DeathAction deathAction = null;
            foreach (var action in actions)
                if (action is DeathAction death)
                {
                    deathAction = death;
                    break;
                }

            Assert.IsNotNull(deathAction, "Should have a death action");
            Assert.AreEqual("Defender", deathAction.Target.Name);
            Assert.IsTrue(deathAction.Target.IsDead);
        }

        [Test]
        public void CombatSystem_WithPoison_ProducesStatusEffectActions()
        {
            var attacker = CreateUnit("Attacker", 100, 5, 0, 10);
            var defender = CreateUnit("Defender", 50, 5, 0, 5);

            // Apply poison to attacker so they take damage each turn
            attacker.ApplyStatus(new Poison(5, 10, 1));

            var actions = CombatSystem.RunFight(attacker, defender);

            var statusActions = new List<StatusEffectAction>();
            foreach (var action in actions)
                if (action is StatusEffectAction status)
                    statusActions.Add(status);

            Assert.Greater(statusActions.Count, 0, "Combat with poison should produce status effect actions");
        }

        [Test]
        public void StatusEffectAction_ContainsCorrectData()
        {
            var unit = CreateUnit("Poisoned", 100, 10, 0, 10);
            var enemy = CreateUnit("Enemy", 100, 5, 0, 5);

            unit.ApplyStatus(new Poison(7, 3, 1));

            var actions = CombatSystem.RunFight(unit, enemy);

            StatusEffectAction statusAction = null;
            foreach (var action in actions)
                if (action is StatusEffectAction status)
                {
                    statusAction = status;
                    break;
                }

            Assert.IsNotNull(statusAction, "Should have a status effect action");
            Assert.AreEqual("Poisoned", statusAction.Target.Name);
            Assert.AreEqual("Poison", statusAction.EffectName);
            Assert.AreEqual(7, statusAction.Amount, "Poison damage should be 7");
        }

        [Test]
        public void CombatSystem_RemainsDeterministic_WithActions()
        {
            // Run the same combat twice and verify results are identical
            var unitA1 = CreateUnit("A", 30, 10, 0, 10);
            var unitB1 = CreateUnit("B", 30, 10, 0, 5);

            var unitA2 = CreateUnit("A", 30, 10, 0, 10);
            var unitB2 = CreateUnit("B", 30, 10, 0, 5);

            var actions1 = CombatSystem.RunFight(unitA1, unitB1);
            var actions2 = CombatSystem.RunFight(unitA2, unitB2);

            Assert.AreEqual(actions1.Count, actions2.Count, "Action count should be identical");
            Assert.AreEqual(unitA1.Stats.CurrentHP, unitA2.Stats.CurrentHP, "Unit A HP should be identical");
            Assert.AreEqual(unitB1.Stats.CurrentHP, unitB2.Stats.CurrentHP, "Unit B HP should be identical");

            // Verify action types match
            for (var i = 0; i < actions1.Count; i++)
                Assert.AreEqual(actions1[i].GetType(), actions2[i].GetType(),
                    $"Action {i} type should match");
        }

        [Test]
        public void CombatSystem_StillAppliesDamage_ToUnits()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 50, 5, 0, 5);

            var initialDefenderHP = defender.Stats.CurrentHP;

            var actions = CombatSystem.RunFight(attacker, defender);

            Assert.Less(defender.Stats.CurrentHP, initialDefenderHP,
                "Combat should still apply damage to units");
            Assert.IsTrue(attacker.Stats.CurrentHP <= 0 || defender.Stats.CurrentHP <= 0,
                "Combat should still end with a winner");
        }
    }
}