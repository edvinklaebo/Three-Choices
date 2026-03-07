using System;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class EnemyFactoryTests
    {
        private EnemyDefinition CreateDefinition(string name, int maxHP, int attackPower,
            int armor, int speed, int minFightIndex = 0, int maxFightIndex = 999)
        {
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.EditorInit(name, maxHP, attackPower, armor, speed, minFightIndex, maxFightIndex);
            return def;
        }

        private EnemyDatabase CreateDatabase(params EnemyDefinition[] definitions)
        {
            var db = ScriptableObject.CreateInstance<EnemyDatabase>();
            foreach (var def in definitions)
                db.EditorAddEnemy(def);
            return db;
        }

        private EnemyFactory CreateFactory(EnemyDatabase database)
        {
            var factory = ScriptableObject.CreateInstance<EnemyFactory>();
            factory.EditorInit(database);
            return factory;
        }

        [Test]
        public void Create_ReturnsUnitWithStatsFromDefinition()
        {
            var def = CreateDefinition("Goblin", maxHP: 25, attackPower: 4, armor: 1, speed: 7);
            var factory = CreateFactory(CreateDatabase(def));

            var enemy = factory.Create(0);

            Assert.IsNotNull(enemy);
            Assert.AreEqual("Goblin", enemy.Name);
            Assert.AreEqual(25, enemy.Stats.MaxHP);
            Assert.AreEqual(25, enemy.Stats.CurrentHP);
            Assert.AreEqual(4, enemy.Stats.AttackPower);
            Assert.AreEqual(1, enemy.Stats.Armor);
            Assert.AreEqual(7, enemy.Stats.Speed);

            Object.DestroyImmediate(factory);
        }

        [Test]
        public void Create_ThrowsInvalidOperationException_WhenNoCandidatesForFightIndex()
        {
            var def = CreateDefinition("HighLevel", maxHP: 50, attackPower: 10, armor: 5, speed: 5,
                minFightIndex: 10, maxFightIndex: 20);
            var factory = CreateFactory(CreateDatabase(def));

            Assert.Throws<InvalidOperationException>(() => factory.Create(0));

            Object.DestroyImmediate(factory);
        }

        [Test]
        public void Create_OnlySelectsCandidatesWithinFightIndexRange()
        {
            var earlyDef = CreateDefinition("EarlyEnemy", maxHP: 10, attackPower: 2, armor: 0, speed: 3,
                minFightIndex: 0, maxFightIndex: 5);
            var lateDef = CreateDefinition("LateEnemy", maxHP: 50, attackPower: 10, armor: 5, speed: 5,
                minFightIndex: 10, maxFightIndex: 20);
            var factory = CreateFactory(CreateDatabase(earlyDef, lateDef));

            // fightIndex 3 is within earlyDef range only
            for (var i = 0; i < 10; i++)
            {
                var enemy = factory.Create(3);
                Assert.AreEqual("EarlyEnemy", enemy.Name,
                    "Only earlyDef should be selected for fightIndex 3");
            }

            Object.DestroyImmediate(factory);
        }

        [Test]
        public void Create_AppliesTraitsToUnit()
        {
            var trait = ScriptableObject.CreateInstance<PoisonOnHitTrait>();
            trait.EditorInit(poisonStacks: 2);

            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.EditorInit("PoisonGoblin", maxHP: 25, attackPower: 4, armor: 1, speed: 7,
                traits: new System.Collections.Generic.List<EnemyTraitDefinition> { trait });

            var factory = CreateFactory(CreateDatabase(def));
            var enemy = factory.Create(0);

            // Verify poison is applied to target when enemy hits
            var target = new Unit("Target") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            enemy.RaiseOnHit(target, 5);

            Assert.AreEqual(1, target.StatusEffects.Count,
                "Target should have one status effect after being hit by a PoisonOnHit enemy");
            Assert.AreEqual("Poison", target.StatusEffects[0].Id);

            Object.DestroyImmediate(factory);
            Object.DestroyImmediate(trait);
        }

        [Test]
        public void Create_DoesNotModifyDefinition()
        {
            var def = CreateDefinition("Goblin", maxHP: 25, attackPower: 4, armor: 1, speed: 7);
            var factory = CreateFactory(CreateDatabase(def));

            factory.Create(0);
            factory.Create(0);

            Assert.AreEqual(25, def.MaxHP, "Definition MaxHP must not be modified by factory");
            Assert.AreEqual(4, def.AttackPower, "Definition AttackPower must not be modified by factory");

            Object.DestroyImmediate(factory);
        }
    }
}