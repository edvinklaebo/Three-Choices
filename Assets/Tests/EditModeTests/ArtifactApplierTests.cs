using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditModeTests
{
    public class ArtifactApplierTests
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
                    CurrentHP = 80,
                    AttackPower = 10,
                    Armor = 5,
                    Speed = 3
                }
            };
        }

        [TearDown]
        public void Teardown()
        {
            DamagePipeline.Clear();
        }

        private ArtifactDefinition CreateStatArtifact(StatType stat, int amount)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit("test", "Test", "desc", Rarity.Common, ArtifactTag.None,
                ArtifactEffectType.StatBoost, stat, amount, "", false);
            return a;
        }

        private ArtifactDefinition CreatePassiveArtifact(string abilityId, ArtifactTag tag = ArtifactTag.None)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit("test", "Test", "desc", Rarity.Common, tag,
                ArtifactEffectType.AddPassive, StatType.MaxHP, 0, abilityId, false);
            return a;
        }

        private ArtifactDefinition CreateAbilityArtifact(string abilityId, ArtifactTag tag = ArtifactTag.None)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit("test", "Test", "desc", Rarity.Common, tag,
                ArtifactEffectType.AddAbility, StatType.MaxHP, 0, abilityId, false);
            return a;
        }

        // ---- NULL GUARD ----

        [Test]
        public void ApplyToPlayer_NullArtifact_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(null, _unit));
        }

        [Test]
        public void ApplyToPlayer_NullPlayer_Throws()
        {
            var artifact = CreateStatArtifact(StatType.MaxHP, 10);
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(artifact, null));
        }

        // ---- STAT BOOSTS ----

        [Test]
        public void StatBoost_MaxHP_IncreasesMaxAndCurrentHP()
        {
            var artifact = CreateStatArtifact(StatType.MaxHP, 20);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(120, _unit.Stats.MaxHP);
            Assert.AreEqual(100, _unit.Stats.CurrentHP);
        }

        [Test]
        public void StatBoost_AttackPower_IncreasesAttack()
        {
            var artifact = CreateStatArtifact(StatType.AttackPower, 7);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(17, _unit.Stats.AttackPower);
        }

        [Test]
        public void StatBoost_Armor_IncreasesArmor()
        {
            var artifact = CreateStatArtifact(StatType.Armor, 4);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(9, _unit.Stats.Armor);
        }

        [Test]
        public void StatBoost_Speed_IncreasesSpeed()
        {
            var artifact = CreateStatArtifact(StatType.Speed, 2);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(5, _unit.Stats.Speed);
        }

        [Test]
        public void StatBoost_UnknownStat_Throws()
        {
            var artifact = CreateStatArtifact((StatType)999, 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        // ---- PASSIVES ----

        [Test]
        public void Passive_Lifesteal_AddsPassiveToUnit()
        {
            var artifact = CreatePassiveArtifact("Lifesteal", ArtifactTag.Lifesteal);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<Lifesteal>(_unit.Passives[0]);
        }

        [Test]
        public void Passive_DoubleStrike_AddsPassiveToUnit()
        {
            var artifact = CreatePassiveArtifact("DoubleStrike", ArtifactTag.DoubleStrike);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<DoubleStrike>(_unit.Passives[0]);
        }

        [Test]
        public void Passive_Thorns_AddsPassiveToUnit()
        {
            var artifact = CreatePassiveArtifact("Thorns", ArtifactTag.Thorns);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<Thorns>(_unit.Passives[0]);
        }

        [Test]
        public void Passive_Rage_LogsCorrectly()
        {
            var artifact = CreatePassiveArtifact("Rage", ArtifactTag.Rage);
            LogAssert.Expect(UnityEngine.LogType.Log, "[INFO] [ArtifactApplier] Passive applied: Rage to Hero");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);
        }

        [Test]
        public void Passive_Poison_AddsPassiveToUnit()
        {
            var artifact = CreatePassiveArtifact("Poison", ArtifactTag.Poison);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<PoisonUpgrade>(_unit.Passives[0]);
        }

        [Test]
        public void Passive_Bleed_AddsPassiveToUnit()
        {
            var artifact = CreatePassiveArtifact("Bleed", ArtifactTag.Bleed);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<BleedUpgrade>(_unit.Passives[0]);
        }

        [Test]
        public void Passive_Unknown_Throws()
        {
            var artifact = CreatePassiveArtifact("GodMode");
            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        // ---- ABILITIES ----

        [Test]
        public void Ability_Fireball_AddsAbilityToUnit()
        {
            var artifact = CreateAbilityArtifact("Fireball", ArtifactTag.Fireball);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<Fireball>(_unit.Abilities[0]);
        }

        [Test]
        public void Ability_ArcaneMissiles_AddsAbilityToUnit()
        {
            var artifact = CreateAbilityArtifact("Arcane Missiles", ArtifactTag.ArcaneMissiles);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Abilities.Count);
            Assert.IsInstanceOf<ArcaneMissiles>(_unit.Abilities[0]);
        }

        [Test]
        public void Ability_Unknown_Throws()
        {
            var artifact = CreateAbilityArtifact("Teleportation");
            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        [Test]
        public void UnknownEffectType_Throws()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Test", "desc", Rarity.Common, ArtifactTag.None,
                (ArtifactEffectType)999, StatType.MaxHP, 0, "", false);

            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        // ---- MULTIPLE ARTIFACTS ----

        [Test]
        public void MultipleArtifacts_AccumulateEffects()
        {
            var hp = CreateStatArtifact(StatType.MaxHP, 20);
            var atk = CreateStatArtifact(StatType.AttackPower, 5);
            var lifesteal = CreatePassiveArtifact("Lifesteal", ArtifactTag.Lifesteal);

            ArtifactApplier.ApplyToPlayer(hp, _unit);
            ArtifactApplier.ApplyToPlayer(atk, _unit);
            ArtifactApplier.ApplyToPlayer(lifesteal, _unit);

            Assert.AreEqual(120, _unit.Stats.MaxHP);
            Assert.AreEqual(15, _unit.Stats.AttackPower);
            Assert.AreEqual(1, _unit.Passives.Count);
        }
    }
}
