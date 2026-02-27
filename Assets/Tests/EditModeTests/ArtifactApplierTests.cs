using System;
using NUnit.Framework;
using UnityEngine;

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

        private ArtifactDefinition CreateArtifactPassive(string abilityId, int amount = 0)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit("test", "Test", "desc", Rarity.Common, ArtifactTag.None,
                ArtifactEffectType.AddArtifact, StatType.MaxHP, amount, abilityId, false);
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
            var artifact = CreateArtifactPassive("PhantomStrike");
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(artifact, null));
        }

        // ---- ARTIFACT PASSIVES ----

        [Test]
        public void Artifact_PhantomStrike_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("PhantomStrike");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_DeathShield_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("DeathShield");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<DeathShield>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_CritChance_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("CritChance", 10);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<CritChancePassive>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_PoisonAmplifier_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("PoisonAmplifier");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<PoisonAmplifier>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_Unknown_Throws()
        {
            var artifact = CreateArtifactPassive("GodMode");
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
            var phantom = CreateArtifactPassive("PhantomStrike");
            var shield = CreateArtifactPassive("DeathShield");

            ArtifactApplier.ApplyToPlayer(phantom, _unit);
            ArtifactApplier.ApplyToPlayer(shield, _unit);

            Assert.AreEqual(2, _unit.Passives.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Passives[0]);
            Assert.IsInstanceOf<DeathShield>(_unit.Passives[1]);
        }
    }
}
