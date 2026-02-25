using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class CombatResultTests
    {
        private static Unit CreateUnit(string name) =>
            new Unit(name) { Stats = new Stats { MaxHP = 10, CurrentHP = 10 } };

        [Test]
        public void Constructor_StoresPlayerEnemyAndActions()
        {
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");
            var actions = new List<ICombatAction>();

            var result = new CombatResult(player, enemy, actions);

            Assert.AreEqual(player, result.Player);
            Assert.AreEqual(enemy, result.Enemy);
            Assert.AreEqual(actions, result.Actions);
        }

        [Test]
        public void Constructor_NullActions_DefaultsToEmptyList()
        {
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            var result = new CombatResult(player, enemy, null);

            Assert.IsNotNull(result.Actions);
            Assert.AreEqual(0, result.Actions.Count);
        }
    }
}
