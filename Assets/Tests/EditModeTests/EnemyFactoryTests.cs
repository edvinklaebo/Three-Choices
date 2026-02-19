using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class EnemyFactoryTests
    {
        [Test]
        public void Create_ReturnsUnitWithCorrectStats()
        {
            const int fightIndex = 3;

            var factory = ScriptableObject.CreateInstance<EnemyFactory>();
            var enemy = factory.Create(fightIndex);

            Assert.IsNotNull(enemy);
            Assert.AreEqual(15 + fightIndex * 5, enemy.Stats.MaxHP);
            Assert.AreEqual(15 + fightIndex * 5, enemy.Stats.CurrentHP);
            Assert.AreEqual(3 + fightIndex, enemy.Stats.AttackPower);
            Assert.AreEqual(2 + fightIndex / 2, enemy.Stats.Armor);
            Assert.AreEqual(5 + fightIndex / 2, enemy.Stats.Speed);
            Assert.IsFalse(string.IsNullOrEmpty(enemy.Name));

            Object.DestroyImmediate(factory);
        }
    }
}