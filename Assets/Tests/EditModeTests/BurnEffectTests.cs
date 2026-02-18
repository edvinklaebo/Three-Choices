using System;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class BurnEffectTests
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
        public void Burn_DealsDamageAtTurnStart()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn = new Burn(3, 5);

            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(95, unit.Stats.CurrentHP, "Burn should deal 5 damage");
        }

        [Test]
        public void Burn_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn = new Burn(3, 5);

            unit.ApplyStatus(burn);

            Assert.AreEqual(3, burn.Duration, "Initial duration should be 3");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, burn.Duration, "Duration should decrease to 2");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, burn.Duration, "Duration should decrease to 1");
        }

        [Test]
        public void Burn_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn = new Burn(2, 5);

            unit.ApplyStatus(burn);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should have 1 status effect");

            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(0, unit.StatusEffects.Count, "Burn should expire after duration reaches 0");
        }

        [Test]
        public void Burn_DoesNotStackDamage_KeepsHighest()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn1 = new Burn(2, 5);
            var burn2 = new Burn(2, 8);

            unit.ApplyStatus(burn1);
            unit.ApplyStatus(burn2);
            
            if(unit.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have 1 burn effect");
            Assert.AreEqual(8, burn.BaseDamage, "Should keep higher damage: 8 > 5");
        }

        [Test]
        public void Burn_DoesNotStackDamage_KeepsHighestWhenLowerApplied()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn1 = new Burn(2, 10);
            var burn2 = new Burn(2, 5);

            unit.ApplyStatus(burn1);
            unit.ApplyStatus(burn2);

            if(unit.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");
            
            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have 1 burn effect");
            Assert.AreEqual(10, burn.BaseDamage, "Should keep higher damage: 10 > 5");
        }

        [Test]
        public void Burn_RefreshesDurationWhenReapplied()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn1 = new Burn(3, 5);

            unit.ApplyStatus(burn1);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(2, burn1.Duration, "Duration should decrease to 2");

            // Reapply burn with same damage
            var burn2 = new Burn(5, 3);
            unit.ApplyStatus(burn2);

            Assert.AreEqual(3, burn1.Duration, "Duration should refresh to 3");
        }

        [Test]
        public void Burn_CanKillUnit()
        {
            var unit = CreateUnit("Test", 10, 0, 0, 5);
            var burn = new Burn(3, 15);

            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart();

            Assert.IsTrue(unit.isDead, "Burn should be able to kill the unit");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");
        }

        [Test]
        public void Burn_BypassesArmor()
        {
            var unit = CreateUnit("Tank", 100, 0, 1000, 5);
            var burn = new Burn(3, 10);

            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(90, unit.Stats.CurrentHP, "Burn should bypass armor and deal full 10 damage");
        }

        [Test]
        public void CombatSystem_TicksBurnAtTurnStart()
        {
            var burned = CreateUnit("Burned", 50, 10, 0, 10);
            var enemy = CreateUnit("Enemy", 100, 5, 0, 5);

            burned.ApplyStatus(new Burn(10, 5));

            CombatSystem.RunFight(burned, enemy);

            // Burned unit should take burn damage each turn
            Assert.LessOrEqual(burned.Stats.CurrentHP, 0, "Burned unit should die");
        }

        [Test]
        public void Burn_KillsBeforeAttack()
        {
            var burned = CreateUnit("Burned", 5, 100, 0, 10);
            var enemy = CreateUnit("Enemy", 10, 0, 0, 5);

            burned.ApplyStatus(new Burn(3, 10));

            CombatSystem.RunFight(burned, enemy);

            Assert.IsTrue(burned.isDead, "Burned unit should die from burn");
            Assert.AreEqual(10, enemy.Stats.CurrentHP,
                "Enemy should take no damage because burned unit died before attacking");
        }

        [Test]
        public void Burn_CanCoexistWithOtherStatusEffects()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var burn = new Burn(3, 5);
            var poison = new Poison(3, 2, 1);

            unit.ApplyStatus(burn);
            unit.ApplyStatus(poison);

            Assert.AreEqual(2, unit.StatusEffects.Count, "Should have 2 different status effects");
        }
    }
}
