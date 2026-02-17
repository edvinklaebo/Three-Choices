using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class ModifierSystemTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
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
            // Clear pipeline before each test
            DamagePipeline.Clear();
        }

        [Test]
        public void DamagePipeline_AppliesModifiersInPriorityOrder()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // Add modifiers with different priorities
            var flatMod = new FlatDamageModifier(attacker, 5); // Priority 10
            var percentMod = new PercentageDamageModifier(attacker, 2f); // Priority 100

            DamagePipeline.Register(flatMod);
            DamagePipeline.Register(percentMod);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            // Expected: (10 + 5) * 2 = 30
            Assert.AreEqual(30, ctx.FinalValue, "Flat modifier should apply before percentage");
        }

        [Test]
        public void DamagePipeline_IncludesPassiveModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // Add Rage passive (Priority 200)
            var rage = new Rage(attacker);
            attacker.Passives.Add(rage);

            // Damage attacker to trigger rage bonus
            attacker.Stats.CurrentHP = 50; // 50% HP = 50% damage bonus

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            // Expected: 10 * 1.5 = 15
            Assert.AreEqual(15, ctx.FinalValue, "Rage should increase damage by 50% at 50% HP");
        }

        [Test]
        public void FlatDamageModifier_AddsBonusDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            var modifier = new FlatDamageModifier(attacker, 7);
            DamagePipeline.Register(modifier);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(17, ctx.FinalValue);
        }

        [Test]
        public void PercentageDamageModifier_MultipliesDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            var modifier = new PercentageDamageModifier(attacker, 1.5f); // +50%
            DamagePipeline.Register(modifier);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(15, ctx.FinalValue);
        }

        [Test]
        public void CriticalHitModifier_CanApplyCriticalDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // 100% crit chance for testing
            var modifier = new CriticalHitModifier(attacker, 1.0f, 2.0f);
            DamagePipeline.Register(modifier);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(20, ctx.FinalValue, "Should deal double damage on crit");
            Assert.IsTrue(ctx.IsCritical, "Context should mark damage as critical");
        }

        [Test]
        public void ExecuteModifier_IncreaseDamageAgainstLowHPTargets()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 10, 0, 5);

            // 50% threshold, 2x damage bonus
            var modifier = new ExecuteModifier(attacker, 0.5f, 2.0f);
            DamagePipeline.Register(modifier);

            // Target at 51% HP - no bonus
            defender.Stats.CurrentHP = 51;
            var ctx1 = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx1);
            Assert.AreEqual(10, ctx1.FinalValue, "No execute bonus above threshold");

            // Target at 50% HP - bonus applies
            defender.Stats.CurrentHP = 50;
            var ctx2 = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx2);
            Assert.AreEqual(20, ctx2.FinalValue, "Execute bonus should apply at threshold");
        }

        [Test]
        public void VulnerabilityModifier_IncreaseDamageToSpecificTarget()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender1 = CreateUnit("Defender1", 100, 0, 0, 5);
            var defender2 = CreateUnit("Defender2", 100, 0, 0, 5);

            // Apply vulnerability to defender1 only
            var modifier = new VulnerabilityModifier(defender1, 1.5f);
            DamagePipeline.Register(modifier);

            var ctx1 = new DamageContext(attacker, defender1, 10);
            DamagePipeline.Process(ctx1);
            Assert.AreEqual(15, ctx1.FinalValue, "Vulnerable target should take 50% more damage");

            var ctx2 = new DamageContext(attacker, defender2, 10);
            DamagePipeline.Process(ctx2);
            Assert.AreEqual(10, ctx2.FinalValue, "Non-vulnerable target should take normal damage");
        }

        [Test]
        public void DamagePipeline_PreventNegativeDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // Apply massive damage reduction
            var modifier = new PercentageDamageModifier(attacker, 0.01f); // -99%
            DamagePipeline.Register(modifier);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.GreaterOrEqual(ctx.FinalValue, 0, "Damage should never be negative");
        }

        [Test]
        public void CombatSystem_IntegratesWithModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 10);
            var defender = CreateUnit("Defender", 50, 0, 0, 5);

            // Add flat damage bonus
            var modifier = new FlatDamageModifier(attacker, 5);
            DamagePipeline.Register(modifier);

            CombatSystem.RunFight(attacker, defender);

            // Attacker should win with bonus damage
            Assert.IsTrue(defender.isDead, "Defender should be defeated");
            Assert.IsFalse(attacker.isDead, "Attacker should survive");
        }

        [Test]
        public void ModifierPriority_CorrectOrderingWithMultipleModifiers()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // Add modifiers in random order
            var lateMod = new PercentageDamageModifier(attacker, 2f); // Priority 100
            var earlyMod = new FlatDamageModifier(attacker, 10); // Priority 10
            var veryLateMod = new CriticalHitModifier(attacker, 1.0f, 1.5f); // Priority 210

            // Register in reverse priority order
            DamagePipeline.Register(veryLateMod);
            DamagePipeline.Register(lateMod);
            DamagePipeline.Register(earlyMod);

            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            // Expected: ((10 + 10) * 2) * 1.5 = 60
            Assert.AreEqual(60, ctx.FinalValue, "Modifiers should apply in priority order regardless of registration order");
        }

        [TearDown]
        public void Teardown()
        {
            DamagePipeline.Clear();
        }
    }
}
