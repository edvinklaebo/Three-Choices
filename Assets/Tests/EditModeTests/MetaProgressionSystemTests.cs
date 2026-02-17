using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class MetaProgressionSystemTests
    {
        private static Unit CreateUnit(string name, int hp, int attack)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = 0,
                    Speed = 5
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            MetaProgressionSystem.Reset();
            DamagePipeline.Clear();
        }

        [Test]
        public void UnlockModifier_AddsToUnlockedList()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var modifier = new FlatDamageModifier(attacker, 5);

            MetaProgressionSystem.UnlockModifier("flat_damage_1", modifier);

            Assert.IsTrue(MetaProgressionSystem.IsUnlocked("flat_damage_1"));
        }

        [Test]
        public void IsUnlocked_ReturnsFalseForUnknownModifier()
        {
            Assert.IsFalse(MetaProgressionSystem.IsUnlocked("nonexistent_modifier"));
        }

        [Test]
        public void ActivateModifier_RegistersWithDamagePipeline()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var defender = CreateUnit("Defender", 100, 0);
            var modifier = new FlatDamageModifier(attacker, 5);

            MetaProgressionSystem.UnlockModifier("flat_damage_1", modifier);
            MetaProgressionSystem.ActivateModifier("flat_damage_1");

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(15, ctx.FinalValue, "Activated modifier should affect damage");
        }

        [Test]
        public void DeactivateAll_RemovesActiveModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var defender = CreateUnit("Defender", 100, 0);
            var modifier = new FlatDamageModifier(attacker, 5);

            MetaProgressionSystem.UnlockModifier("flat_damage_1", modifier);
            MetaProgressionSystem.ActivateModifier("flat_damage_1");
            MetaProgressionSystem.DeactivateAll();

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Deactivated modifier should not affect damage");
        }

        [Test]
        public void GetUnlockedModifiers_ReturnsAllUnlocked()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var mod1 = new FlatDamageModifier(attacker, 5);
            var mod2 = new PercentageDamageModifier(attacker, 1.5f);

            MetaProgressionSystem.UnlockModifier("mod1", mod1);
            MetaProgressionSystem.UnlockModifier("mod2", mod2);

            var unlocked = MetaProgressionSystem.GetUnlockedModifiers();
            var unlockedList = new System.Collections.Generic.List<string>(unlocked);

            Assert.AreEqual(2, unlockedList.Count);
            Assert.Contains("mod1", unlockedList);
            Assert.Contains("mod2", unlockedList);
        }

        [Test]
        public void Reset_ClearsAllModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var modifier = new FlatDamageModifier(attacker, 5);

            MetaProgressionSystem.UnlockModifier("flat_damage_1", modifier);
            MetaProgressionSystem.ActivateModifier("flat_damage_1");
            MetaProgressionSystem.Reset();

            Assert.IsFalse(MetaProgressionSystem.IsUnlocked("flat_damage_1"));
        }

        [Test]
        public void ActivateModifier_IgnoresUnknownModifier()
        {
            // Should not throw or crash
            MetaProgressionSystem.ActivateModifier("unknown_modifier");
        }

        [Test]
        public void SaveAndLoad_PersistsUnlockedModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10);
            var mod1 = new FlatDamageModifier(attacker, 5);
            var mod2 = new PercentageDamageModifier(attacker, 1.5f);

            // Unlock and save
            MetaProgressionSystem.UnlockModifier("mod1", mod1);
            MetaProgressionSystem.UnlockModifier("mod2", mod2);
            MetaProgressionSystem.Save();

            // Clear in-memory state
            MetaProgressionSystem.Reset();
            Assert.IsFalse(MetaProgressionSystem.IsUnlocked("mod1"));

            // Load and verify IDs are restored
            var loadedIds = MetaProgressionSystem.Load();
            Assert.AreEqual(2, loadedIds.Count);
            Assert.Contains("mod1", loadedIds);
            Assert.Contains("mod2", loadedIds);

            // Re-register modifiers with their IDs
            MetaProgressionSystem.RegisterUnlockedModifier("mod1", mod1);
            MetaProgressionSystem.RegisterUnlockedModifier("mod2", mod2);

            // Verify they work after reload
            Assert.IsTrue(MetaProgressionSystem.IsUnlocked("mod1"));
            Assert.IsTrue(MetaProgressionSystem.IsUnlocked("mod2"));
        }

        [TearDown]
        public void Teardown()
        {
            MetaProgressionSystem.Reset();
            DamagePipeline.Clear();
        }
    }
}
