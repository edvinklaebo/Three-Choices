using NUnit.Framework;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="CombatActionLog"/> in isolation.
    /// Verifies that actions are recorded in order and the read-only view reflects additions.
    /// </summary>
    public class CombatActionLogTests
    {
        private static Unit CreateUnit(string name) => new Unit(name)
        {
            Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 10 }
        };

        [Test]
        public void Actions_IsEmpty_WhenNothingAdded()
        {
            var log = new CombatActionLog();

            Assert.IsEmpty(log.Actions);
        }

        [Test]
        public void Add_AppendsAction_ToLog()
        {
            var log = new CombatActionLog();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var action = new DamageAction(source, target, 10, 100, 90, 100);

            log.Add(action);

            Assert.AreEqual(1, log.Actions.Count);
            Assert.AreEqual(action, log.Actions[0]);
        }

        [Test]
        public void Add_MultipleActions_PreservesInsertionOrder()
        {
            var log = new CombatActionLog();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var first = new DamageAction(source, target, 5, 100, 95, 100);
            var second = new DamageAction(source, target, 10, 95, 85, 100);

            log.Add(first);
            log.Add(second);

            Assert.AreEqual(2, log.Actions.Count);
            Assert.AreEqual(first, log.Actions[0]);
            Assert.AreEqual(second, log.Actions[1]);
        }

        [Test]
        public void Actions_ExposesReadOnlyView()
        {
            var log = new CombatActionLog();

            Assert.IsInstanceOf<System.Collections.Generic.IReadOnlyList<ICombatAction>>(log.Actions);
        }
    }
}
