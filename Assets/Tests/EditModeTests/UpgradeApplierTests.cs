using System;
using Core;
using Core.Abilities;
using Core.Combat;
using Core.Passives;
using Core.Passives.Definitions;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class UpgradeApplierTests
    {
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
            _unit = new Unit("Hero")
            {
                Stats = new Stats
                {
                    MaxHP = 100,
                    CurrentHP = 50,
                    AttackPower = 10,
                    Armor = 5,
                    Speed = 3
                }
            };
        }

        // ---------- DISPATCH TESTS ----------

        [Test]
        public void Apply_StatUpgrade_DispatchesToStatHandler()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();
            upgrade.EditorInit("A", "A", StatType.AttackPower, 5);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(15, _unit.Stats.AttackPower);
        }

        // ---------- STAT UPGRADES ----------

        [Test]
        public void ApplyStat_MaxHP_IncreasesMaxAndCurrentHP()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();

            upgrade.EditorInit("A", "A", StatType.MaxHP, 20);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(120, _unit.Stats.MaxHP);
            Assert.AreEqual(70, _unit.Stats.CurrentHP);
        }

        [Test]
        public void ApplyStat_AttackPower_IncreasesAttack()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();

            upgrade.EditorInit("A", "A", StatType.AttackPower, 7);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(17, _unit.Stats.AttackPower);
        }

        [Test]
        public void ApplyStat_Armor_IncreasesArmor()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();

            upgrade.EditorInit("A", "A", StatType.Armor, 4);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(9, _unit.Stats.Armor);
        }

        [Test]
        public void ApplyStat_Speed_IncreasesSpeed()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();

            upgrade.EditorInit("A", "A", StatType.Speed, 2);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(5, _unit.Stats.Speed);
        }

        [Test]
        public void ApplyStat_UnknownStat_Throws()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();

            upgrade.EditorInit("A", "A", (StatType)999, 10);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                UpgradeApplier.Apply(upgrade, _unit));
        }

        // ---------- ABILITY UPGRADES ----------

        [Test]
        public void ApplyAbility_Fireball_FirstApplication_AddsAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<FireballDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 10, damagePerUpgrade: 5);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<Fireball>(_unit.Abilities[0]);
        }

        [Test]
        public void ApplyAbility_Fireball_DuplicateApplication_DoesNotAddAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<FireballDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 10, damagePerUpgrade: 5);

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count, "Duplicate Fireball upgrade should not add a second ability");
        }

        [Test]
        public void ApplyAbility_Fireball_DuplicateApplication_Adds5Damage()
        {
            var upgrade = ScriptableObject.CreateInstance<FireballDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 10, damagePerUpgrade: 5);

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            var caster = new Unit("Caster") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var target = new Unit("Target") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var fireball = (Fireball)_unit.Abilities[0];
            fireball.OnCast(caster, target, new CombatContext());

            // baseDamage=10, DamagePerUpgrade=5 → after one upgrade: 15
            Assert.AreEqual(85, target.Stats.CurrentHP, "Fireball should deal 15 damage after one duplicate upgrade");
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_FirstApplication_AddsAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<ArcaneMissilesDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 5, damagePerUpgrade: 1, missileCount: 3);

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<ArcaneMissiles>(_unit.Abilities[0]);
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_DuplicateApplication_DoesNotAddAbility()
        {
            var upgrade = ScriptableObject.CreateInstance<ArcaneMissilesDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 5, damagePerUpgrade: 1, missileCount: 3);

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count, "Duplicate Arcane Missiles upgrade should not add a second ability");
        }

        [Test]
        public void ApplyAbility_ArcaneMissiles_DuplicateApplication_Adds1Damage()
        {
            var upgrade = ScriptableObject.CreateInstance<ArcaneMissilesDefinition>();
            upgrade.EditorInit("A", "A", baseDamage: 5, damagePerUpgrade: 1, missileCount: 3);

            UpgradeApplier.Apply(upgrade, _unit);
            UpgradeApplier.Apply(upgrade, _unit);

            var caster = new Unit("Caster") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var target = new Unit("Target") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } };
            var missiles = (ArcaneMissiles)_unit.Abilities[0];
            missiles.OnCast(caster, target, new CombatContext());

            // baseDamage=5, DamagePerUpgrade=1, missileCount=3 → after one upgrade: 6 × 3 = 18
            Assert.AreEqual(82, target.Stats.CurrentHP, "Arcane Missiles should deal 18 damage after one duplicate upgrade");
        }
        
        [Test]
        public void ApplyPassive_Thorns_AddsThornsToUnit()
        {
            var upgrade = ScriptableObject.CreateInstance<ThornsDefinition>();
            upgrade.EditorInit("thorns", "Thorns");

            UpgradeApplier.Apply(upgrade, _unit);

            Assert.AreEqual(1, _unit.Passives.Count, "ThornsDefinition should add one passive");
            Assert.IsInstanceOf<Thorns>(_unit.Passives[0], "The passive added should be a Thorns instance");
        }
    }
}