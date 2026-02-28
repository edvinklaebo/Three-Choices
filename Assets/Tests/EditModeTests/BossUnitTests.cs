using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class BossUnitTests
    {
        private static BossDefinition CreateDefinition(string id = "test_boss",
            string displayName = "Test Boss",
            int maxHP = 200, int attackPower = 20, int armor = 5, int speed = 8)
        {
            var def = ScriptableObject.CreateInstance<BossDefinition>();
            def.EditorInit(id, displayName, 1, stats: new Stats
            {
                MaxHP = maxHP,
                CurrentHP = maxHP,
                AttackPower = attackPower,
                Armor = armor,
                Speed = speed
            });
            return def;
        }

        [Test]
        public void Boss_Constructor_SetsNameFromDefinition()
        {
            var def = CreateDefinition(displayName: "Dragon");
            var boss = new Boss(def);
            Assert.AreEqual("Dragon", boss.Name);
        }

        [Test]
        public void Boss_Constructor_ThrowsOnNullDefinition()
        {
            Assert.Throws<System.ArgumentNullException>(() => _ = new Boss(null));
        }

        [Test]
        public void Boss_Constructor_CopiesMaxHp()
        {
            var def = CreateDefinition(maxHP: 500);
            var boss = new Boss(def);
            Assert.AreEqual(500, boss.Stats.MaxHP);
        }

        [Test]
        public void Boss_Constructor_StartsAtFullHp()
        {
            var def = CreateDefinition(maxHP: 300);
            var boss = new Boss(def);
            Assert.AreEqual(300, boss.Stats.CurrentHP);
        }

        [Test]
        public void Boss_Constructor_CopiesAttackPower()
        {
            var def = CreateDefinition(attackPower: 42);
            var boss = new Boss(def);
            Assert.AreEqual(42, boss.Stats.AttackPower);
        }

        [Test]
        public void Boss_Constructor_CopiesArmor()
        {
            var def = CreateDefinition(armor: 15);
            var boss = new Boss(def);
            Assert.AreEqual(15, boss.Stats.Armor);
        }

        [Test]
        public void Boss_Constructor_CopiesSpeed()
        {
            var def = CreateDefinition(speed: 12);
            var boss = new Boss(def);
            Assert.AreEqual(12, boss.Stats.Speed);
        }

        [Test]
        public void Boss_Constructor_ExposesDefinition()
        {
            var def = CreateDefinition(id: "my_boss");
            var boss = new Boss(def);
            Assert.AreEqual(def, boss.Definition);
        }

        [Test]
        public void Boss_IsUnit_AndCanTakeDamage()
        {
            var def = CreateDefinition(maxHP: 100);
            var boss = new Boss(def);
            var attacker = new Unit("attacker") { Stats = new Stats { AttackPower = 10 } };

            boss.ApplyDamage(attacker, 30);

            Assert.AreEqual(70, boss.Stats.CurrentHP);
        }

        [Test]
        public void Boss_DefaultStats_WhenDefinitionHasNoStats()
        {
            var def = ScriptableObject.CreateInstance<BossDefinition>();
            def.EditorInit("empty_boss", "Empty", 1);

            var boss = new Boss(def);

            Assert.AreEqual(0, boss.Stats.MaxHP);
            Assert.AreEqual(0, boss.Stats.CurrentHP);
        }
    }
}
