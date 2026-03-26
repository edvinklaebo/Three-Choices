using NUnit.Framework;

using Core;
using Core.Passives;
using Core.Passives.Definitions;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class RageTests
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

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            DamagePipeline.Clear();
        }

        // ---- At full HP ----

        [Test]
        public void Rage_DoesNotIncreaseDamage_AtFullHP()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(unit, 1.0f);
            rage.OnAttach(unit);
            unit.Passives.Add(rage);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "At full HP missing percent = 0, bonus multiplier should be 1.0");
        }

        // ---- At 50 % HP ----

        [Test]
        public void Rage_IncreasesDamage_AtHalfHP()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 50; // 50 % missing
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(unit, 1.0f); // maxBonus = 1.0
            rage.OnAttach(unit);
            unit.Passives.Add(rage);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // bonus = 1 + 0.5 * 1.0 = 1.5 → 10 * 1.5 = 15
            Assert.AreEqual(15, ctx.FinalValue, "At 50 % HP bonus multiplier should be 1.5 → 10 * 1.5 = 15");
        }

        // ---- At 0 HP edge case (cap) ----

        [Test]
        public void Rage_AppliesMaxBonus_AtZeroHP()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 0; // 100 % missing
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(unit, 1.0f);
            rage.OnAttach(unit);
            unit.Passives.Add(rage);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // bonus = 1 + 1.0 * 1.0 = 2.0 → 10 * 2.0 = 20
            Assert.AreEqual(20, ctx.FinalValue, "At 0 HP bonus multiplier should be 2.0 → 10 * 2.0 = 20");
        }

        // ---- Custom maxBonus ----

        [Test]
        public void Rage_UsesConfiguredMaxBonus()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 0; // 100 % missing
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(unit, 0.5f); // maxBonus = 0.5
            rage.OnAttach(unit);
            unit.Passives.Add(rage);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // bonus = 1 + 1.0 * 0.5 = 1.5 → 10 * 1.5 = 15
            Assert.AreEqual(15, ctx.FinalValue, "maxBonus=0.5 at 0 HP should give multiplier 1.5 → 15");
        }

        // ---- Does not affect other units ----

        [Test]
        public void Rage_DoesNotAffectOtherUnits()
        {
            var rager = CreateUnit("Rager", 100, 10, 0, 10);
            rager.Stats.CurrentHP = 0;
            var otherAttacker = CreateUnit("Other", 100, 10, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(rager, 1.0f);
            rage.OnAttach(rager);
            rager.Passives.Add(rage);

            var ctx = new DamageContext(otherAttacker, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Rage should not affect other units' damage");
        }

        // ---- OnDetach ----

        [Test]
        public void Rage_NoLongerModifiesDamage_AfterDetach()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 0;
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var rage = new Rage(unit, 1.0f);
            rage.OnAttach(unit);
            unit.Passives.Add(rage);

            rage.OnDetach(unit);
            unit.Passives.Remove(rage);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Detached Rage should not modify damage");
        }

        // ---- Definition: data-driven balance ----

        [Test]
        public void RageDefinition_CreatesPassive_WithConfiguredMaxBonus()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 0; // 100 % missing
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<RageDefinition>();
            definition.EditorInit("rage", "Rage", maxBonus: 0.5f);
            definition.Apply(unit);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // bonus = 1 + 1.0 * 0.5 = 1.5 → 10 * 1.5 = 15
            Assert.AreEqual(15, ctx.FinalValue, "Definition-configured maxBonus=0.5 should give multiplier 1.5 → 15");

            ScriptableObject.DestroyImmediate(definition);
        }

        [Test]
        public void RageDefinition_DefaultMaxBonus_Is100Percent()
        {
            var unit = CreateUnit("Rager", 100, 10, 0, 10);
            unit.Stats.CurrentHP = 0; // 100 % missing
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<RageDefinition>();
            definition.EditorInit("rage", "Rage"); // default maxBonus = 1.0
            definition.Apply(unit);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // bonus = 1 + 1.0 * 1.0 = 2.0 → 10 * 2.0 = 20
            Assert.AreEqual(20, ctx.FinalValue, "Default maxBonus=1.0 at 0 HP should double damage → 20");

            ScriptableObject.DestroyImmediate(definition);
        }
    }
}
