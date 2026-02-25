using NUnit.Framework;
using System.Linq;

namespace Tests.EditModeTests
{
    public class ThornsTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
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

            // Attacker hits defender — thorns should deal Armor/2 = 10 back
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(100 - 10, attacker.Stats.CurrentHP, "Thorns should deal half armor (10) to attacker");
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
    }
}
