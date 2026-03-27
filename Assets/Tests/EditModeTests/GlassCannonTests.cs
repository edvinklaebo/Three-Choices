using NUnit.Framework;

using Core;
using Core.Passives;
using Core.Passives.Definitions;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class GlassCannonTests
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

        // ---- Outgoing damage ----

        [Test]
        public void GlassCannon_IncreasesOutgoingDamage_By60Percent()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(cannon, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(16, ctx.FinalValue, "10 * 1.6 = 16");
        }

        [Test]
        public void GlassCannon_OutgoingDamage_RoundsUp()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(cannon, target, 7);
            DamagePipeline.Process(ctx);

            // 7 * 1.6 = 11.2 -> ceil = 12
            Assert.AreEqual(12, ctx.FinalValue, "7 * 1.6 = 11.2, ceiled to 12");
        }

        // ---- Incoming damage ----

        [Test]
        public void GlassCannon_IncreasesIncomingDamage_By40Percent()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(attacker, cannon, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(14, ctx.FinalValue, "10 * 1.4 = 14");
        }

        [Test]
        public void GlassCannon_IncomingDamage_RoundsUp()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(attacker, cannon, 5);
            DamagePipeline.Process(ctx);

            // 5 * 1.4 = 7.0 -> 7
            Assert.AreEqual(7, ctx.FinalValue, "5 * 1.4 = 7");
        }

        // ---- No effect on unrelated units ----

        [Test]
        public void GlassCannon_DoesNotAffectOtherUnits_OutgoingDamage()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var otherAttacker = CreateUnit("Other", 100, 10, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(otherAttacker, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Glass Cannon should not affect other units' outgoing damage");
        }

        [Test]
        public void GlassCannon_DoesNotAffectOtherUnits_IncomingDamage()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var otherTarget = CreateUnit("OtherTarget", 100, 0, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            var ctx = new DamageContext(attacker, otherTarget, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Glass Cannon should not affect other units' incoming damage");
        }

        // ---- Integration: full combat ----

        [Test]
        public void GlassCannon_DealsMoreDamage_InCombat()
        {
            var cannonUnit = CreateUnit("Cannon", 1000, 10, 0, 10);
            var baseline = CreateUnit("Baseline", 1000, 10, 0, 10);
            var enemyForCannon = CreateUnit("EnemyA", 1000, 0, 0, 5);
            var enemyForBaseline = CreateUnit("EnemyB", 1000, 0, 0, 5);

            var passive = new GlassCannon(cannonUnit, 0.6f, 0.4f);
            passive.OnAttach(cannonUnit);
            cannonUnit.Passives.Add(passive);

            var ctxCannon = new DamageContext(cannonUnit, enemyForCannon, 10);
            var ctxBaseline = new DamageContext(baseline, enemyForBaseline, 10);
            DamagePipeline.Process(ctxCannon);
            DamagePipeline.Process(ctxBaseline);

            Assert.Greater(ctxCannon.FinalValue, ctxBaseline.FinalValue,
                "Glass Cannon unit should deal more damage than baseline");
        }

        [Test]
        public void GlassCannon_TakesMoreDamage_InCombat()
        {
            var cannonUnit = CreateUnit("Cannon", 200, 0, 0, 5);
            var baseline = CreateUnit("Baseline", 200, 0, 0, 5);
            var attackerForCannon = CreateUnit("AttackerA", 100, 10, 0, 10);
            var attackerForBaseline = CreateUnit("AttackerB", 100, 10, 0, 10);

            var passive = new GlassCannon(cannonUnit, 0.6f, 0.4f);
            passive.OnAttach(cannonUnit);
            cannonUnit.Passives.Add(passive);

            var ctxCannon = new DamageContext(attackerForCannon, cannonUnit, 10);
            var ctxBaseline = new DamageContext(attackerForBaseline, baseline, 10);
            DamagePipeline.Process(ctxCannon);
            DamagePipeline.Process(ctxBaseline);

            Assert.Greater(ctxCannon.FinalValue, ctxBaseline.FinalValue,
                "Glass Cannon unit should take more damage than baseline");
        }

        // ---- OnDetach ----

        [Test]
        public void GlassCannon_NoLongerModifiesDamage_AfterDetach()
        {
            var cannon = CreateUnit("Cannon", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var passive = new GlassCannon(cannon, 0.6f, 0.4f);
            passive.OnAttach(cannon);
            cannon.Passives.Add(passive);

            passive.OnDetach(cannon);
            cannon.Passives.Remove(passive);

            var ctx = new DamageContext(cannon, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Detached Glass Cannon should not modify damage");
        }

        // ---- Definition: data-driven constructor ----

        [Test]
        public void GlassCannonDefinition_CreatesPassive_WithConfiguredValues()
        {
            var unit = CreateUnit("Cannon", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<GlassCannonDefinition>();
            definition.EditorInit("glass_cannon", "Glass Cannon", outgoingBonus: 1.0f, incomingPenalty: 0.5f);
            definition.Apply(unit);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // 10 * (1 + 1.0) = 20
            Assert.AreEqual(20, ctx.FinalValue, "Definition-configured outgoingBonus=1.0 should double damage");

            ScriptableObject.DestroyImmediate(definition);
        }

        [Test]
        public void GlassCannonDefinition_DefaultValues_Are60And40Percent()
        {
            var unit = CreateUnit("Cannon", 100, 10, 0, 10);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<GlassCannonDefinition>();
            definition.EditorInit("glass_cannon", "Glass Cannon");
            definition.Apply(unit);

            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(16, ctx.FinalValue, "Default outgoingBonus=0.6 should give 10 * 1.6 = 16");

            ScriptableObject.DestroyImmediate(definition);
        }
    }
}
