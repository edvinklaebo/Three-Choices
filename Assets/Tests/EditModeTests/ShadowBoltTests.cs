using Core;
using Core.Abilities;
using Core.Combat;
using Core.StatusEffects;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    public class ShadowBoltTests
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

        [Test]
        public void ShadowBolt_DealsDamageToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(baseDamage: 8);
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(92, target.Stats.CurrentHP, "Shadow Bolt should deal 8 damage to the target");
        }

        [Test]
        public void ShadowBolt_AppliesWeakToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate();
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(1, target.StatusEffects.Count, "Shadow Bolt should apply one status effect");
            Assert.AreEqual("Weak", target.StatusEffects[0].Id, "Status effect should be Weak");
        }

        [Test]
        public void ShadowBolt_WeakHasCorrectStacks()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(weakStacks: 2, weakDuration: 3);
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(2, target.StatusEffects[0].Stacks, "Weak should have the configured stack count");
            Assert.AreEqual(3, target.StatusEffects[0].Duration, "Weak should have the configured duration");
        }

        [Test]
        public void ShadowBolt_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDamage(caster, 100);

            Assert.IsTrue(target.IsDead, "Target should be dead");

            var shadowBolt = ShadowBolt.EditorCreate();
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(0, target.StatusEffects.Count, "Dead target should not receive Weak");
        }

        [Test]
        public void ShadowBolt_WeakReducesTargetOutgoingDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var weakenedUnit = CreateUnit("WeakenedUnit", 100, 10, 0, 5);
            var victim = CreateUnit("Victim", 100, 0, 0, 5);

            // Apply Weak (1 stack, damageReductionPerStack defaults to 1) to weakenedUnit
            var shadowBolt = ShadowBolt.EditorCreate(weakStacks: 1, weakDuration: 5);
            var context = new CombatContext();
            shadowBolt.OnCast(caster, weakenedUnit, context);

            Assert.AreEqual("Weak", weakenedUnit.StatusEffects[0].Id, "WeakenedUnit should have Weak");

            // WeakenedUnit attacks victim — Weak reduces outgoing damage by 1 stack × 1 reduction = 1
            context.DealDamage(weakenedUnit, victim, 10);

            // 10 base damage - 1 Weak reduction = 9 damage dealt
            Assert.AreEqual(91, victim.Stats.CurrentHP, "Weak should reduce weakened unit's outgoing damage by 1");
        }

        [Test]
        public void ShadowBolt_CooldownPreventsSecondCast()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Create with 1-round cooldown
            var definition = UnityEngine.ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("_editor", "_editor", baseDamage: 10, damagePerUpgrade: 2, cooldownRounds: 1);
            var shadowBolt = new ShadowBolt(definition);

            var context = new CombatContext();

            // First cast: should deal 10 damage and start cooldown
            shadowBolt.OnCast(caster, target, context);
            Assert.AreEqual(90, target.Stats.CurrentHP, "First cast should deal 10 damage");
            Assert.AreEqual(1, target.StatusEffects.Count, "Weak should be applied on first cast");

            // Second cast (same turn): cooldown active, should be skipped
            shadowBolt.OnCast(caster, target, context);
            Assert.AreEqual(90, target.Stats.CurrentHP, "Second cast should be skipped due to cooldown");
            Assert.AreEqual(1, target.StatusEffects.Count, "No additional Weak should be applied during cooldown");
        }

        [Test]
        public void ShadowBolt_AddDamage_IncreasesOutputDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(baseDamage: 8);
            shadowBolt.AddDamage(4);

            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(88, target.Stats.CurrentHP, "Shadow Bolt should deal 12 damage after AddDamage(4)");
        }

        [Test]
        public void ShadowBolt_TriggersAtTurnStartInCombat()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 10);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            caster.Abilities.Add(ShadowBolt.EditorCreate(baseDamage: 50));

            CombatSystem.RunFight(caster, target);

            Assert.IsTrue(target.IsDead, "Target should be dead from Shadow Bolt damage");
            Assert.Greater(caster.Stats.CurrentHP, 0, "Caster should survive (target has 0 attack)");
        }

        [Test]
        public void ShadowBolt_HasLowerPriorityThanFireball()
        {
            var shadowBolt = ShadowBolt.EditorCreate();
            var fireball = Fireball.EditorCreate();

            Assert.Less(shadowBolt.Priority, fireball.Priority,
                "Shadow Bolt (30) should have lower priority than Fireball (50)");
        }

        [Test]
        public void ShadowBoltDefinition_Apply_AddsAbilityToUnit()
        {
            var unit = CreateUnit("Player", 100, 10, 0, 5);

            var definition = UnityEngine.ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("shadow_bolt", "Shadow Bolt");
            definition.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count, "Apply should add Shadow Bolt to the unit");
            Assert.IsInstanceOf<ShadowBolt>(unit.Abilities[0]);
        }

        [Test]
        public void ShadowBoltDefinition_Apply_StacksDamageOnDuplicate()
        {
            var unit = CreateUnit("Player", 100, 10, 0, 5);

            var definition = UnityEngine.ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("shadow_bolt", "Shadow Bolt", baseDamage: 8, damagePerUpgrade: 2);
            definition.Apply(unit);
            definition.Apply(unit); // second pickup

            Assert.AreEqual(1, unit.Abilities.Count, "Duplicate pickup should not add a second ability instance");

            // Verify damage increased: cast once and check 8+2=10 damage
            var target = CreateUnit("Target", 100, 0, 0, 5);
            var context = new CombatContext();
            unit.Abilities[0].OnCast(unit, target, context);

            Assert.AreEqual(90, target.Stats.CurrentHP, "Second pickup should increase damage by DamagePerUpgrade (8+2=10)");
        }
    }
}
