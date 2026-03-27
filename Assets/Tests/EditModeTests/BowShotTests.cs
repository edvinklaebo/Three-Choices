using Core;
using Core.Abilities;
using Core.Combat;
using Core.StatusEffects;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class BowShotTests
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

        private static BleedDefinition CreateBleedDefinition(int stacks = 2, int duration = 3, int baseDamage = 2)
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks, duration, baseDamage);
            return data;
        }

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
        }

        // ---- Damage ----

        [Test]
        public void BowShot_DealsDamageToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bowShot = BowShot.EditorCreate(baseDamage: 8);
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual(92, target.Stats.CurrentHP, "Bow Shot should deal 8 damage to the target");
        }

        [Test]
        public void BowShot_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDamage(caster, 100);

            Assert.IsTrue(target.IsDead, "Target should be dead");

            var bowShot = BowShot.EditorCreate();
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual(0, target.StatusEffects.Count, "Dead target should not receive Bleed");
        }

        // ---- Bleed application ----

        [Test]
        public void BowShot_AppliesBleedToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bowShot = BowShot.EditorCreate();
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual(1, target.StatusEffects.Count, "Bow Shot should apply one status effect");
            Assert.AreEqual("Bleed", target.StatusEffects[0].Id, "The status effect should be Bleed");
        }

        [Test]
        public void BowShot_AppliesCorrectBleedStacks()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bleedDef = CreateBleedDefinition(stacks: 3);
            var bowShot = BowShot.EditorCreate(bleedDefinition: bleedDef);
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual(3, target.StatusEffects[0].Stacks, "Bow Shot should apply 3 Bleed stacks");

            Object.DestroyImmediate(bleedDef);
        }

        [Test]
        public void BowShot_AppliesCorrectBleedDuration()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bleedDef = CreateBleedDefinition(duration: 4);
            var bowShot = BowShot.EditorCreate(bleedDefinition: bleedDef);
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual(4, target.StatusEffects[0].Duration, "Bow Shot should apply Bleed with duration 4");

            Object.DestroyImmediate(bleedDef);
        }

        [Test]
        public void BowShot_UsesCodeDefaults_WhenBleedDefinitionIsNull()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bowShot = BowShot.EditorCreate(bleedDefinition: null);
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            Assert.AreEqual("Bleed", target.StatusEffects[0].Id, "Bleed should be applied using code defaults");
            Assert.Greater(target.StatusEffects[0].Stacks, 0, "Bleed stacks should be positive");
            Assert.Greater(target.StatusEffects[0].Duration, 0, "Bleed duration should be positive");
        }

        // ---- Upgrade stacking ----

        [Test]
        public void BowShot_AddDamage_IncreasesBaseDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bowShot = BowShot.EditorCreate(baseDamage: 8);
            bowShot.AddDamage(3);

            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            // 8 base + 3 added = 11 damage
            Assert.AreEqual(89, target.Stats.CurrentHP,
                "AddDamage should increase Bow Shot damage by the given amount");
        }

        [Test]
        public void BowShotDefinition_Apply_CreatesNewAbilityOnFirstPickup()
        {
            var unit = CreateUnit("Player", 100, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<BowShotDefinition>();
            definition.EditorInit("bow_shot", "Bow Shot");
            definition.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count, "First pickup should add Bow Shot to the unit");
            Assert.IsInstanceOf<BowShot>(unit.Abilities[0]);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void BowShotDefinition_Apply_StacksDamageOnDuplicatePickup()
        {
            var caster = CreateUnit("Player", 100, 0, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var definition = ScriptableObject.CreateInstance<BowShotDefinition>();
            definition.EditorInit("bow_shot", "Bow Shot", baseDamage: 8, damagePerUpgrade: 5);

            // First pickup
            definition.Apply(caster);
            // Second pickup
            definition.Apply(caster);

            Assert.AreEqual(1, caster.Abilities.Count, "Duplicate pickup should not add a second Bow Shot");

            var context = new CombatContext();
            caster.Abilities[0].OnCast(caster, target, context);

            // 8 base + 5 upgrade = 13
            Assert.AreEqual(200 - 13, target.Stats.CurrentHP,
                "Duplicate pickup should stack damage via AddDamage");

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void BowShotDefinition_UsesBleedDefinition_WhenSet()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bleedDef = CreateBleedDefinition(stacks: 5, duration: 4, baseDamage: 3);
            var definition = ScriptableObject.CreateInstance<BowShotDefinition>();
            definition.EditorInit("bow_shot", "Bow Shot", bleedDefinition: bleedDef);
            definition.Apply(caster);

            var context = new CombatContext();
            caster.Abilities[0].OnCast(caster, target, context);

            Assert.AreEqual(5, target.StatusEffects[0].Stacks, "Bleed stacks should come from BleedDefinition asset");
            Assert.AreEqual(4, target.StatusEffects[0].Duration, "Bleed duration should come from BleedDefinition asset");
            Assert.AreEqual(3, target.StatusEffects[0].BaseDamage, "Bleed base damage should come from BleedDefinition asset");

            Object.DestroyImmediate(bleedDef);
            Object.DestroyImmediate(definition);
        }

        // ---- Integration ----

        [Test]
        public void BowShot_TriggersInCombat_AndBleedsTarget()
        {
            var caster = CreateUnit("Caster", 100, 50, 0, 10);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            caster.Abilities.Add(BowShot.EditorCreate(baseDamage: 10));

            CombatSystem.RunFight(caster, target);

            Assert.IsTrue(target.IsDead, "Target should be dead from Bow Shot + attacks");
            Assert.Greater(caster.Stats.CurrentHP, 0, "Caster should survive (target has 0 attack)");
        }

        [Test]
        public void BowShot_ProducesBowShotAction_NotDamageAction()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var bowShot = BowShot.EditorCreate();
            var context = new CombatContext();
            bowShot.OnCast(caster, target, context);

            var bowShotActions = 0;
            var damageActions = 0;
            foreach (var action in context.Actions)
            {
                switch (action)
                {
                    case BowShotAction:
                        bowShotActions++;
                        break;
                    case DamageAction:
                        damageActions++;
                        break;
                }
            }

            Assert.AreEqual(1, bowShotActions, "Bow Shot should produce exactly one BowShotAction");
            Assert.AreEqual(0, damageActions, "Bow Shot should not produce a plain DamageAction");
        }
    }
}
