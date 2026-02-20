using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class CombatLoggerTests
    {
        private static Unit CreateUnit(string name, int hp = 100, int attack = 10, int armor = 0, int speed = 5)
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
        public void StatusEffectApplied_RaisedWithStackedFalse_OnFirstApplication()
        {
            var unit = CreateUnit("Test");
            Unit receivedUnit = null;
            IStatusEffect receivedEffect = null;
            var receivedStacked = true;

            unit.StatusEffectApplied += (u, e, stacked) =>
            {
                receivedUnit = u;
                receivedEffect = e;
                receivedStacked = stacked;
            };

            var poison = new Poison(5, 3, 1);
            unit.ApplyStatus(poison);

            Assert.AreSame(unit, receivedUnit);
            Assert.AreSame(poison, receivedEffect);
            Assert.IsFalse(receivedStacked, "First application should not be stacked");
        }

        [Test]
        public void StatusEffectApplied_RaisedWithStackedTrue_OnSecondApplication()
        {
            var unit = CreateUnit("Test");
            var stackedValues = new System.Collections.Generic.List<bool>();

            unit.StatusEffectApplied += (_, _, stacked) => stackedValues.Add(stacked);

            unit.ApplyStatus(new Poison(5, 3, 1));
            unit.ApplyStatus(new Poison(3, 2, 1));

            Assert.AreEqual(2, stackedValues.Count);
            Assert.IsFalse(stackedValues[0], "First application should not be stacked");
            Assert.IsTrue(stackedValues[1], "Second application should be stacked");
        }

        [Test]
        public void StatusEffectApplied_ReportsUpdatedStackCount_OnStack()
        {
            var unit = CreateUnit("Test");
            IStatusEffect lastEffect = null;

            unit.StatusEffectApplied += (_, e, _) => lastEffect = e;

            unit.ApplyStatus(new Poison(3, 3, 1));
            unit.ApplyStatus(new Poison(4, 3, 1));

            Assert.IsNotNull(lastEffect);
            Assert.AreEqual(7, lastEffect.Stacks, "Stacked event should report combined stack count");
        }

        [Test]
        public void RegisterUnit_ReceivesDamagedEvent()
        {
            var attacker = CreateUnit("Attacker");
            var defender = CreateUnit("Defender");

            var damagedAttacker = (Unit)null;
            var damagedAmount = 0;

            defender.Damaged += (a, d) =>
            {
                damagedAttacker = a;
                damagedAmount = d;
            };

            CombatLogger.Instance.RegisterUnit(defender);
            defender.ApplyDamage(attacker, 15);

            Assert.AreSame(attacker, damagedAttacker);
            Assert.AreEqual(15, damagedAmount);
        }

        [Test]
        public void RegisterUnit_ReceivesHealthChangedEvent()
        {
            var unit = CreateUnit("Test", hp: 50);
            var healthChangedFired = false;

            unit.HealthChanged += (_, _, _) => healthChangedFired = true;

            CombatLogger.Instance.RegisterUnit(unit);
            unit.Heal(10);

            Assert.IsTrue(healthChangedFired);
        }

        [Test]
        public void RegisterUnit_ReceivesDiedEvent()
        {
            var victim = CreateUnit("Test", hp: 10);
            var rapist = CreateUnit("Test", hp: 10);
            
            var diedFired = false;

            victim.Died += _ => diedFired = true;

            CombatLogger.Instance.RegisterUnit(victim);
            victim.ApplyDamage(rapist, 10);

            Assert.IsTrue(diedFired);
        }

        [Test]
        public void RegisterUnit_ReceivesStatusEffectAppliedEvent()
        {
            var unit = CreateUnit("Test");
            var statusFired = false;

            unit.StatusEffectApplied += (_, _, _) => statusFired = true;

            CombatLogger.Instance.RegisterUnit(unit);
            unit.ApplyStatus(new Poison(5, 3, 1));

            Assert.IsTrue(statusFired);
        }
    }
}
