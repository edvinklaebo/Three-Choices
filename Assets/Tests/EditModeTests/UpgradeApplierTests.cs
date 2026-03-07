using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditModeTests
{
    public class UpgradeApplierTests
    {
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
            _unit = new Unit("Hero");
            _unit.Stats = new Stats
            {
                MaxHP = 100,
                CurrentHP = 50,
                AttackPower = 10,
                Armor = 5,
                Speed = 3
            };
        }

        // ---------- DISPATCH TESTS ----------

        [Test]
        public void Apply_StatUpgrade_DispatchesToStatHandler()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Stat, StatType.AttackPower, 5);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(15, _unit.Stats.AttackPower);
        }

        [Test]
        public void Apply_UnknownUpgradeType_Throws()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", (UpgradeType)999, StatType.AttackPower, 999);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }

        // ---------- STAT UPGRADES ----------

        [Test]
        public void ApplyStat_MaxHP_IncreasesMaxAndCurrentHP()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Stat, StatType.MaxHP, 20);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(120, _unit.Stats.MaxHP);
            Assert.AreEqual(70, _unit.Stats.CurrentHP);
        }

        [Test]
        public void ApplyStat_AttackPower_IncreasesAttack()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Stat, StatType.AttackPower, 7);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(17, _unit.Stats.AttackPower);
        }

        [Test]
        public void ApplyStat_Armor_IncreasesArmor()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Stat, StatType.Armor, 4);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(9, _unit.Stats.Armor);
        }

        [Test]
        public void ApplyStat_Speed_IncreasesSpeed()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Stat, StatType.Speed, 2);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(5, _unit.Stats.Speed);
        }

        [Test]
        public void ApplyStat_UnknownStat_Throws()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Stat, (StatType)999, 10);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }

        // ---------- ABILITY UPGRADES ----------

        [Test]
        public void ApplyAbility_Fireball_FirstApplication_AddsAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Fireball");

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<Fireball>(_unit.Abilities[0]);
        }

        [Test]
        public void ApplyAbility_Fireball_DuplicateApplication_DoesNotAddAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Fireball");

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count, "Duplicate Fireball upgrade should not add a second ability");
        }

        [Test]
        public void ApplyAbility_Fireball_DuplicateApplication_Adds5Damage()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Fireball");

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            var caster = new Unit("Caster") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var target = new Unit("Target") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var fireball = (Fireball)_unit.Abilities[0];
            fireball.OnCast(caster, target, new CombatContext());

            // Default damage is 10, +5 from duplicate = 15
            Assert.AreEqual(85, target.Stats.CurrentHP, "Fireball should deal 15 damage after one duplicate upgrade");
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_FirstApplication_AddsAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Arcane Missiles");

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<ArcaneMissiles>(_unit.Abilities[0]);
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_DuplicateApplication_DoesNotAddAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Arcane Missiles");

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count, "Duplicate Arcane Missiles upgrade should not add a second ability");
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_DuplicateApplication_Adds1Damage()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Arcane Missiles");

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            var caster = new Unit("Caster") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var target = new Unit("Target") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var missiles = (ArcaneMissiles)_unit.Abilities[0];
            missiles.OnCast(caster, target, new CombatContext());

            // Default: 5 damage × 3 missiles = 15. After +1 damage: 6 × 3 = 18
            Assert.AreEqual(82, target.Stats.CurrentHP, "Arcane Missiles should deal 18 damage after one duplicate upgrade");
        }

        [Test]
        public void ApplyAbility_AlwaysThrows_ForUnknownAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Ability, "Turd blaster");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }

        // ---------- PASSIVE UPGRADES ----------

        [Test]
        public void ApplyPassive_Thorns_LogsCorrectly()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Passive, "Thorns");

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Thorns");

            UpgradeApplier.Apply(upgrade, _unit);
        }

        [Test]
        public void ApplyPassive_Rage_LogsCorrectly()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Passive, "Rage");

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Rage");

            UpgradeApplier.Apply(upgrade, _unit);
        }

        [Test]
        public void ApplyPassive_Lifesteal_LogsCorrectly()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Passive, "Lifesteal");

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Lifesteal");

            UpgradeApplier.Apply(upgrade, _unit);
        }

        [Test]
        public void ApplyPassive_Poison_LogsCorrectly()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Passive, "Poison");

            LogAssert.Expect(LogType.Log, "[INFO] Passive Applied: Poison");

            UpgradeApplier.Apply(upgrade, _unit);
        }

        [Test]
        public void ApplyAbility_UnknownAbility_Throws()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Ability, "GodMode");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }

        [Test]
        public void ApplyPassive_UnknownPassive_Throws()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();

            upgrade.EditorInit("A", "A", UpgradeType.Passive, "GodMode");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }
    }
}