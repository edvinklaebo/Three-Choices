using NUnit.Framework;
using System.Linq;

using Core;
using Core.Passives;
using Core.Passives.Definitions;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class ThornsTests
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

        private static Thorns AttachThorns(Unit unit)
        {
            var thorns = new Thorns();
            thorns.OnAttach(unit);
            unit.Passives.Add(thorns);
            return thorns;
        }

        [Test]
        public void Thorns_DealsHalfArmorDamage_ToAttacker()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 100, 0, 20, 5);

            AttachThorns(defender);

            // Run through the combat pipeline so the OnHitEvent path is exercised
            var actions = CombatSystem.RunFight(attacker, defender);

            // Verify the ThornsAction carries Armor/2 = 10 damage
            var thornsAction = actions.OfType<ThornsAction>().FirstOrDefault(a => a.Target == attacker);
            Assert.IsNotNull(thornsAction, "Thorns should produce a ThornsAction targeting the attacker");
            Assert.AreEqual(10, thornsAction.Amount, "Thorns should deal half armor (10) to attacker");
        }

        [Test]
        public void Thorns_DoesNotReflect_WhenAttackerIsNull()
        {
            var defender = CreateUnit("Defender", 100, 0, 20, 5);
            AttachThorns(defender);

            // Damage from status effect (attacker = null) — no reflection
            defender.ApplyDamage(null, 5);

            // Defender took damage, nobody to reflect to — just ensure no exception
            Assert.AreEqual(95, defender.Stats.CurrentHP, "Defender should still take damage from null attacker");
        }

        [Test]
        public void Thorns_DoesNotReflect_WhenArmorIsZero()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 100, 0, 0, 5); // no armour

            AttachThorns(defender);

            var attackerHpBefore = attacker.Stats.CurrentHP;
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(attackerHpBefore, attacker.Stats.CurrentHP, "Thorns should not deal damage when armor is 0");
        }

        [Test]
        public void Thorns_DoesNotCauseInfiniteLoop_WhenBothUnitsHaveThorns()
        {
            var unitA = CreateUnit("Unit A", 100, 10, 20, 10);
            var unitB = CreateUnit("Unit B", 100, 10, 20, 5);

            AttachThorns(unitA);
            AttachThorns(unitB);

            // This should terminate and not throw a StackOverflowException
            Assert.DoesNotThrow(() => CombatSystem.RunFight(unitA, unitB),
                "Combat with Thorns on both units should not cause infinite recursion");
        }

        [Test]
        public void Thorns_Reflects_EachAttackSeparately()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 1000, 0, 10, 5); // high HP so fight lasts

            AttachThorns(defender);

            // Run a full fight and confirm attacker took thorn damage (Armor/2 = 5) per hit
            var actions = CombatSystem.RunFight(attacker, defender);

            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.IsNotEmpty(damageActions, "There should be damage actions in the fight");

            // Attacker HP should be lower than max due to thorn damage
            Assert.Less(attacker.Stats.CurrentHP, 100, "Attacker should have taken thorn damage during the fight");
        }

        [Test]
        public void Thorns_OnDetach_StopsReflecting()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 100, 0, 20, 5);

            var thorns = AttachThorns(defender);
            thorns.OnDetach(defender);

            var attackerHpBefore = attacker.Stats.CurrentHP;
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(attackerHpBefore, attacker.Stats.CurrentHP, "Thorns should not reflect after being detached");
        }

        [Test]
        public void Thorns_CreatesActionEvent_ForReflectDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 1000, 0, 10, 5); // Armor/2 = 5 thorn damage

            AttachThorns(defender);

            var actions = CombatSystem.RunFight(attacker, defender);

            // Thorn reflect must produce a ThornsAction (not a plain DamageAction) targeting the attacker.
            var thornsActions = actions.OfType<ThornsAction>()
                .Where(a => a.Target == attacker)
                .ToList();

            Assert.IsNotEmpty(thornsActions,
                "Thorn reflect must produce at least one ThornsAction targeting the attacker so the shake animation and damage are displayed");
        }

        // ---- Definition: data-driven balance ----

        [Test]
        public void ThornsDefinition_CreatesPassive_WithConfiguredMultiplier()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 1000, 0, 20, 5); // Armor = 20

            var definition = ScriptableObject.CreateInstance<ThornsDefinition>();
            definition.EditorInit("thorns", "Thorns", armorMultiplier: 1.0f); // reflect full armor
            definition.Apply(defender);

            var actions = CombatSystem.RunFight(attacker, defender);

            var thornsAction = actions.OfType<ThornsAction>().FirstOrDefault(a => a.Target == attacker);
            Assert.IsNotNull(thornsAction, "Thorns should produce a ThornsAction targeting the attacker");
            Assert.AreEqual(20, thornsAction.Amount, "armorMultiplier=1.0 should reflect full armor (20)");

            ScriptableObject.DestroyImmediate(definition);
        }

        [Test]
        public void ThornsDefinition_DefaultMultiplier_IsHalfArmor()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 1000, 0, 20, 5); // Armor = 20

            var definition = ScriptableObject.CreateInstance<ThornsDefinition>();
            definition.EditorInit("thorns", "Thorns"); // default 0.5
            definition.Apply(defender);

            var actions = CombatSystem.RunFight(attacker, defender);

            var thornsAction = actions.OfType<ThornsAction>().FirstOrDefault(a => a.Target == attacker);
            Assert.IsNotNull(thornsAction, "Thorns should produce a ThornsAction targeting the attacker");
            Assert.AreEqual(10, thornsAction.Amount, "Default armorMultiplier=0.5 should reflect half armor (10)");

            ScriptableObject.DestroyImmediate(definition);
        }
    }
}
