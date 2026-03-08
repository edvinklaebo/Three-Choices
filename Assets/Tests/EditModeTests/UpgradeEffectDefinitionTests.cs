using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditModeTests
{
    public class UpgradeEffectDefinitionTests
    {
        private Unit CreateUnit()
        {
            DamagePipeline.Clear();
            var unit = new Unit("Hero");
            unit.Stats = new Stats
            {
                MaxHP = 100,
                CurrentHP = 60,
                AttackPower = 10,
                Armor = 5,
                Speed = 3
            };
            return unit;
        }

        // ---- StatsDefinition ----

        [Test]
        public void StatsDefinition_Apply_MaxHP_IncreasesMaxAndCurrentHP()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            def.EditorInit(StatType.MaxHP, 20);
            var unit = CreateUnit();

            def.Apply(unit);

            Assert.AreEqual(120, unit.Stats.MaxHP);
            Assert.AreEqual(80, unit.Stats.CurrentHP);
        }

        [Test]
        public void StatsDefinition_Apply_AttackPower_IncreasesAttack()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            def.EditorInit(StatType.AttackPower, 7);
            var unit = CreateUnit();

            def.Apply(unit);

            Assert.AreEqual(17, unit.Stats.AttackPower);
        }

        [Test]
        public void StatsDefinition_Apply_Armor_IncreasesArmor()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            def.EditorInit(StatType.Armor, 4);
            var unit = CreateUnit();

            def.Apply(unit);

            Assert.AreEqual(9, unit.Stats.Armor);
        }

        [Test]
        public void StatsDefinition_Apply_Speed_IncreasesSpeed()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            def.EditorInit(StatType.Speed, 2);
            var unit = CreateUnit();

            def.Apply(unit);

            Assert.AreEqual(5, unit.Stats.Speed);
        }

        [Test]
        public void StatsDefinition_Apply_UnknownStat_Throws()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            def.EditorInit((StatType)999, 10);
            var unit = CreateUnit();

            Assert.Throws<ArgumentOutOfRangeException>(() => def.Apply(unit));
        }

        [Test]
        public void StatsDefinition_IsAssignableFrom_UpgradeEffectDefinition()
        {
            var def = ScriptableObject.CreateInstance<StatsDefinition>();
            Assert.IsInstanceOf<UpgradeEffectDefinition>(def);
        }

        // ---- PassiveDefinition ----

        [Test]
        public void PassiveDefinition_Apply_Thorns_AddsPassiveToUnit()
        {
            var def = ScriptableObject.CreateInstance<PassiveDefinition>();
            def.EditorInit(AbilityId.Thorns);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Thorns");
            def.Apply(unit);

            Assert.AreEqual(1, unit.Passives.Count);
            Assert.IsInstanceOf<Thorns>(unit.Passives[0]);
        }

        [Test]
        public void PassiveDefinition_Apply_Lifesteal_AddsPassiveToUnit()
        {
            var def = ScriptableObject.CreateInstance<PassiveDefinition>();
            def.EditorInit(AbilityId.Lifesteal);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Lifesteal");
            def.Apply(unit);

            Assert.AreEqual(1, unit.Passives.Count);
            Assert.IsInstanceOf<Lifesteal>(unit.Passives[0]);
        }

        [Test]
        public void PassiveDefinition_Apply_Rage_LogsCorrectly()
        {
            var def = ScriptableObject.CreateInstance<PassiveDefinition>();
            def.EditorInit(AbilityId.Rage);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Rage");
            def.Apply(unit);
        }

        [Test]
        public void PassiveDefinition_Apply_UnknownPassive_Throws()
        {
            var def = ScriptableObject.CreateInstance<PassiveDefinition>();
            def.EditorInit((AbilityId)999);
            var unit = CreateUnit();

            Assert.Throws<ArgumentOutOfRangeException>(() => def.Apply(unit));
        }

        [Test]
        public void PassiveDefinition_IsAssignableFrom_UpgradeEffectDefinition()
        {
            var def = ScriptableObject.CreateInstance<PassiveDefinition>();
            Assert.IsInstanceOf<UpgradeEffectDefinition>(def);
        }

        // ---- AbilityDefinition ----

        [Test]
        public void AbilityDefinition_Apply_Fireball_AddsAbilityToUnit()
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.EditorInit(AbilityId.Fireball);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Ability Applied: Fireball");
            def.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count);
            Assert.IsInstanceOf<Fireball>(unit.Abilities[0]);
        }

        [Test]
        public void AbilityDefinition_Apply_Fireball_Duplicate_DoesNotAddSecondAbility()
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.EditorInit(AbilityId.Fireball);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Ability Applied: Fireball");
            def.Apply(unit);
            LogAssert.Expect(LogType.Log, "[INFO] Ability Applied: Fireball");
            def.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count);
        }

        [Test]
        public void AbilityDefinition_Apply_ArcaneMissiles_AddsAbilityToUnit()
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.EditorInit(AbilityId.ArcaneMissiles);
            var unit = CreateUnit();

            LogAssert.Expect(LogType.Log, "[INFO] Ability Applied: Arcane Missiles");
            def.Apply(unit);

            Assert.AreEqual(1, unit.Abilities.Count);
            Assert.IsInstanceOf<ArcaneMissiles>(unit.Abilities[0]);
        }

        [Test]
        public void AbilityDefinition_Apply_UnknownAbility_Throws()
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.EditorInit((AbilityId)999);
            var unit = CreateUnit();

            Assert.Throws<ArgumentOutOfRangeException>(() => def.Apply(unit));
        }

        [Test]
        public void AbilityDefinition_IsAssignableFrom_UpgradeEffectDefinition()
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            Assert.IsInstanceOf<UpgradeEffectDefinition>(def);
        }
    }
}
