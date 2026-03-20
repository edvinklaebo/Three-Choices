using Core;
using Core.Abilities;
using Core.Abilities.Definitions;
using Core.Combat;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    /// <summary>
    ///     Tests for the data-driven ability system:
    ///     <see cref="FireballData"/>, <see cref="ArcaneMissilesData"/> ScriptableObjects
    ///     and the runtime modifiers that can be applied without touching those assets.
    /// </summary>
    public class AbilityDataTests
    {
        private static Unit CreateUnit(string name, int hp, int armor = 0)
            => new Unit(name) { Stats = new Stats { MaxHP = hp, CurrentHP = hp, Armor = armor } };

        [SetUp]
        public void Setup() => DamagePipeline.Clear();

        // ---- FireballData construction ----

        [Test]
        public void FireballData_CreateRuntimeAbility_ReturnFireball()
        {
            var data = ScriptableObject.CreateInstance<FireballData>();
            var ability = data.CreateRuntimeAbility();
            Assert.IsInstanceOf<Fireball>(ability);
        }

        [Test]
        public void FireballData_Ctor_SeedsBaseDamageFromData()
        {
            // Arrange: create data asset with known base damage
            var data = ScriptableObject.CreateInstance<FireballData>();
            data.EditorInit(baseDamage: 4, damagePerUpgrade: 2, cooldownRounds: 0,
                            burnDuration: 3, burnDamagePercent: 0.5f);

            var fireball = new Fireball(data);
            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);

            fireball.OnCast(caster, target, new CombatContext());

            Assert.AreEqual(96, target.Stats.CurrentHP, "Fireball should deal 4 damage (from FireballData.BaseDamage)");
        }

        // ---- ArcaneMissilesData construction ----

        [Test]
        public void ArcaneMissilesData_CreateRuntimeAbility_ReturnsArcaneMissiles()
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            var ability = data.CreateRuntimeAbility();
            Assert.IsInstanceOf<ArcaneMissiles>(ability);
        }

        [Test]
        public void ArcaneMissilesData_Ctor_SeedsMissileCountAndDamageFromData()
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            data.EditorInit(baseDamage: 2, damagePerUpgrade: 1, cooldownRounds: 0, missileCount: 4);

            var missiles = new ArcaneMissiles(data);
            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);

            missiles.OnCast(caster, target, new CombatContext());

            // 4 missiles × 2 damage = 8
            Assert.AreEqual(92, target.Stats.CurrentHP, "4 missiles at 2 damage = 8 total");
        }

        // ---- AddDamage (single modifier method) ----

        [Test]
        public void Fireball_AddDamage_IncreasesEffectiveDamageWithoutTouchingSO()
        {
            var data = ScriptableObject.CreateInstance<FireballData>();
            data.EditorInit(baseDamage: 4, damagePerUpgrade: 2, cooldownRounds: 0,
                            burnDuration: 3, burnDamagePercent: 0.5f);

            var fireball = new Fireball(data);
            // Simulate "Fireball deals +2 damage" via AddDamage
            fireball.AddDamage(2);

            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);
            fireball.OnCast(caster, target, new CombatContext());

            // base 4 + added 2 = 6 damage
            Assert.AreEqual(94, target.Stats.CurrentHP, "AddDamage(2) should increase 4 base to 6 total");
            // Confirm SO was not mutated
            Assert.AreEqual(4, data.BaseDamage, "ScriptableObject BaseDamage must not change");
        }

        [Test]
        public void ArcaneMissiles_AddDamage_IncreasesPerMissileDamageWithoutTouchingSO()
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            data.EditorInit(baseDamage: 2, damagePerUpgrade: 1, cooldownRounds: 0, missileCount: 3);

            var missiles = new ArcaneMissiles(data);
            // Simulate "Missiles deal +1 damage" via AddDamage
            missiles.AddDamage(1);

            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);
            missiles.OnCast(caster, target, new CombatContext());

            // (2 + 1) × 3 = 9 damage
            Assert.AreEqual(91, target.Stats.CurrentHP, "(2 base + 1 added) × 3 missiles = 9 damage");
            Assert.AreEqual(2, data.BaseDamage, "ScriptableObject BaseDamage must not change");
        }

        [Test]
        public void ArcaneMissiles_AddMissile_IncreasesSalvoSize()
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            data.EditorInit(baseDamage: 3, damagePerUpgrade: 1, cooldownRounds: 0, missileCount: 2);

            var missiles = new ArcaneMissiles(data);
            missiles.AddMissile(1); // +1 extra missile

            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);
            missiles.OnCast(caster, target, new CombatContext());

            // 3 missiles × 3 damage = 9
            Assert.AreEqual(91, target.Stats.CurrentHP, "3 missiles × 3 damage = 9 total");
        }

        // ---- Cooldown ----

        [Test]
        public void Fireball_WithCooldown_SkipsTurnsBeforeNextCast()
        {
            var data = ScriptableObject.CreateInstance<FireballData>();
            data.EditorInit(baseDamage: 5, damagePerUpgrade: 1, cooldownRounds: 1,
                            burnDuration: 1, burnDamagePercent: 0f);

            var fireball = new Fireball(data);
            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);
            var ctx = new CombatContext();

            // Turn 1: fires (cooldown 0 → sets to 1)
            fireball.OnCast(caster, target, ctx);
            Assert.AreEqual(95, target.Stats.CurrentHP, "Turn 1: should deal 5 damage");

            // Turn 2: on cooldown (1 → decrements to 0, skips)
            fireball.OnCast(caster, target, ctx);
            Assert.AreEqual(95, target.Stats.CurrentHP, "Turn 2: on cooldown, should deal no damage");

            // Turn 3: ready again (cooldown 0)
            fireball.OnCast(caster, target, ctx);
            Assert.AreEqual(90, target.Stats.CurrentHP, "Turn 3: should deal 5 damage again");
        }

        [Test]
        public void ArcaneMissiles_WithCooldown_SkipsTurnsBeforeNextCast()
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            data.EditorInit(baseDamage: 2, damagePerUpgrade: 1, cooldownRounds: 2, missileCount: 1);

            var missiles = new ArcaneMissiles(data);
            var caster = CreateUnit("Caster", 100);
            var target = CreateUnit("Target", 100);
            var ctx = new CombatContext();

            // Turn 1: fires
            missiles.OnCast(caster, target, ctx);
            Assert.AreEqual(98, target.Stats.CurrentHP, "Turn 1: should deal 2 damage");

            // Turns 2 & 3: on cooldown
            missiles.OnCast(caster, target, ctx);
            Assert.AreEqual(98, target.Stats.CurrentHP, "Turn 2: still on cooldown");
            missiles.OnCast(caster, target, ctx);
            Assert.AreEqual(98, target.Stats.CurrentHP, "Turn 3: still on cooldown");

            // Turn 4: ready
            missiles.OnCast(caster, target, ctx);
            Assert.AreEqual(96, target.Stats.CurrentHP, "Turn 4: fires again");
        }

        // ---- Tags ----

        [Test]
        public void AbilityData_Tags_AreReadableFromConfig()
        {
            var data = ScriptableObject.CreateInstance<FireballData>();
            data.EditorInitTags(new[] { "fire", "projectile" });

            Assert.AreEqual(2, data.Tags.Count);
            Assert.IsTrue(data.Tags.Contains("fire"));
            Assert.IsTrue(data.Tags.Contains("projectile"));
        }
    }
}
