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

        // ---- Damage ----

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

        // ---- Weak application ----

        [Test]
        public void ShadowBolt_AppliesWeakToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate();
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(1, target.StatusEffects.Count, "Shadow Bolt should apply one status effect");
            Assert.AreEqual("Weak", target.StatusEffects[0].Id, "The status effect should be Weak");
        }

        [Test]
        public void ShadowBolt_AppliesCorrectWeakStacks()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(weakStacks: 3);
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(3, target.StatusEffects[0].Stacks, "Shadow Bolt should apply 3 Weak stacks");
        }

        [Test]
        public void ShadowBolt_AppliesCorrectWeakDuration()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(weakDuration: 4);
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            Assert.AreEqual(4, target.StatusEffects[0].Duration, "Shadow Bolt should apply Weak with duration 4");
        }

        // ---- Upgrade stacking ----

        [Test]
        public void ShadowBolt_AddDamage_IncreasesBaseDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate(baseDamage: 8);
            shadowBolt.AddDamage(3);

            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            // 8 base + 3 added = 11 damage
            Assert.AreEqual(89, target.Stats.CurrentHP,
                "AddDamage should increase Shadow Bolt damage by the given amount");
        }

        [Test]
        public void ShadowBoltDefinition_Apply_CreatesNewAbilityOnFirstPickup()
        {
            var unit = CreateUnit("Player", 100, 0, 0, 5);

            var definition = UnityEngine.ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("shadow_bolt", "Shadow Bolt");
            definition.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count, "First pickup should add Shadow Bolt to the unit");
            Assert.IsInstanceOf<ShadowBolt>(unit.Abilities[0]);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void ShadowBoltDefinition_Apply_StacksDamageOnDuplicatePickup()
        {
            var caster = CreateUnit("Player", 100, 0, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = UnityEngine.ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("shadow_bolt", "Shadow Bolt", baseDamage: 8, damagePerUpgrade: 5);

            // First pickup
            definition.Apply(caster);
            // Second pickup
            definition.Apply(caster);

            Assert.AreEqual(1, caster.Abilities.Count, "Duplicate pickup should not add a second Shadow Bolt");

            var context = new CombatContext();
            caster.Abilities[0].OnCast(caster, target, context);

            // 8 base + 5 upgrade = 13
            Assert.AreEqual(200 - 13, target.Stats.CurrentHP,
                "Duplicate pickup should stack damage via AddDamage");

            UnityEngine.Object.DestroyImmediate(definition);
        }

        // ---- Integration ----

        [Test]
        public void ShadowBolt_TriggersInCombat_AndWeakensTarget()
        {
            var caster = CreateUnit("Caster", 100, 50, 0, 10);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            caster.Abilities.Add(ShadowBolt.EditorCreate(baseDamage: 10));

            CombatSystem.RunFight(caster, target);

            Assert.IsTrue(target.IsDead, "Target should be dead from Shadow Bolt + attacks");
            Assert.Greater(caster.Stats.CurrentHP, 0, "Caster should survive (target has 0 attack)");
        }

        [Test]
        public void ShadowBolt_HasLowerPriorityThanArcaneMissiles()
        {
            var shadowBolt = ShadowBolt.EditorCreate();
            var arcaneMissiles = ArcaneMissiles.EditorCreate();

            Assert.Less(shadowBolt.Priority, arcaneMissiles.Priority,
                "Shadow Bolt (30) should have lower priority than Arcane Missiles (40)");
        }

        [Test]
        public void ShadowBolt_ProducesShadowBoltAction_NotDamageAction()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var shadowBolt = ShadowBolt.EditorCreate();
            var context = new CombatContext();
            shadowBolt.OnCast(caster, target, context);

            var shadowBoltActions = 0;
            var damageActions = 0;
            foreach (var action in context.Actions)
            {
                if (action is ShadowBoltAction) shadowBoltActions++;
                else if (action is DamageAction) damageActions++;
            }

            Assert.AreEqual(1, shadowBoltActions, "Shadow Bolt should produce exactly one ShadowBoltAction");
            Assert.AreEqual(0, damageActions, "Shadow Bolt should not produce a plain DamageAction");
        }
    }
}
