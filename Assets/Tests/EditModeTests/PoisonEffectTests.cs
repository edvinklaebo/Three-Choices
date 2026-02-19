using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class PoisonEffectTests
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
        public void Poison_DealsDamageAtTurnStart()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new Poison(5, 3, 1);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(95, unit.Stats.CurrentHP, "Poison should deal 5 damage");
        }

        [Test]
        public void Poison_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new Poison(5, 3, 1);

            unit.ApplyStatus(poison);

            Assert.AreEqual(3, poison.Duration, "Initial duration should be 3");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, poison.Duration, "Duration should decrease to 2");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, poison.Duration, "Duration should decrease to 1");
        }

        [Test]
        public void Poison_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new Poison(5, 2, 1);

            unit.ApplyStatus(poison);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should have 1 status effect");

            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(0, unit.StatusEffects.Count, "Poison should expire after duration reaches 0");
        }

        [Test]
        public void Poison_StacksAddOnReapply()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison1 = new Poison(3, 2, 1);
            var poison2 = new Poison(2, 2, 1);

            unit.ApplyStatus(poison1);
            unit.ApplyStatus(poison2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have 1 poison effect");
            Assert.AreEqual(5, poison1.Stacks, "Stacks should add: 3 + 2 = 5");
        }

        [Test]
        public void Poison_CanKillUnit()
        {
            var unit = CreateUnit("Test", 10, 0, 0, 5);
            var poison = new Poison(15, 3, 1);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.IsTrue(unit.isDead, "Poison should be able to kill the unit");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");
        }

        [Test]
        public void Poison_BypassesArmor()
        {
            var unit = CreateUnit("Tank", 100, 0, 1000, 5);
            var poison = new Poison(10, 3, 1);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(90, unit.Stats.CurrentHP, "Poison should bypass armor and deal full 10 damage");
        }

        [Test]
        public void PoisonUpgrade_AppliesPoisonToTargetOnHit()
        {
            var attacker = CreateUnit("Attacker", 100, 1, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.Passives.Add(new PoisonUpgrade(attacker));

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count, "Defender should have poison status effect");
            var poison = defender.StatusEffects[0];
            Assert.AreEqual("Poison", poison.Id);
            Assert.AreEqual(2, poison.Stacks, "Poison should have 2 stacks");
            Assert.AreEqual(3, poison.Duration, "Poison should have 3 turns duration");
        }

        [Test]
        public void PoisonUpgrade_StacksOnMultipleHits()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.Passives.Add(new PoisonUpgrade(attacker));

            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(2, defender.StatusEffects[0].Stacks);

            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(4, defender.StatusEffects[0].Stacks, "Poison stacks should accumulate to 4");
            Assert.AreEqual(1, defender.StatusEffects.Count, "Should still only have one poison effect");
        }

        [Test]
        public void PoisonUpgrade_IsConfigurable()
        {
            var attacker = CreateUnit("Attacker", 100, 1, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.Passives.Add(new PoisonUpgrade(attacker, stacks: 5, duration: 6, baseDamage: 3));

            defender.ApplyDamage(attacker, 10);

            var poison = defender.StatusEffects[0];
            Assert.AreEqual(5, poison.Stacks, "Poison stacks should match configured value");
            Assert.AreEqual(6, poison.Duration, "Poison duration should match configured value");
            Assert.AreEqual(3, poison.BaseDamage, "Poison base damage should match configured value");
        }

        [Test]
        public void PoisonUpgrade_WorksInCombat()
        {
            var attacker = CreateUnit("Attacker", 100, 5, 0, 10);
            var defender = CreateUnit("Defender", 20, 0, 0, 5);

            attacker.Passives.Add(new PoisonUpgrade(attacker, 3, 5));

            CombatSystem.RunFight(attacker, defender);

            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should be dead");
            Assert.AreEqual(100, attacker.Stats.CurrentHP,
                "Attacker should take no damage since defender has 0 attack");
        }

        [Test]
        public void PoisonUpgrade_OnDetach_StopsApplyingPoison()
        {
            var attacker = CreateUnit("Attacker", 100, 1, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            var upgrade = new PoisonUpgrade(attacker);
            attacker.Passives.Add(upgrade);

            upgrade.OnDetach(attacker);

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(0, defender.StatusEffects.Count, "No poison after detach");
        }
    }
}
