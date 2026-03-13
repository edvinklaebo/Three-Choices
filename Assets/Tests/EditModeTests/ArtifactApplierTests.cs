using System;

using Core;
using Core.Artifacts;
using Core.Artifacts.Definitions;
using Core.Artifacts.Passives;

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

        // ---- NULL GUARD ----

        [Test]
        public void ApplyToPlayer_NullArtifact_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(null, _unit));
        }

        [Test]
        public void ApplyToPlayer_NullPlayer_Throws()
        {
            var artifact = ScriptableObject.CreateInstance<PhantomStrikeDefinition>();
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(artifact, null));
        }

        // ---- ARTIFACT PASSIVES ----

        [Test]
        public void Artifact_PhantomStrike_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<PhantomStrikeDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_DeathShield_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<DeathShieldDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<DeathShield>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_CritChance_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<CritChanceDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<CritChance>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_PoisonAmplifier_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<PoisonAmplifierDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PoisonAmplifier>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BerserkerMask_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<BerserkerMaskDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BerserkerMask>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BlazingTorch_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<BlazingTorchDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BlazingTorch>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BloodRitual_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<BloodRitualDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BloodRitual>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_CorruptedTome_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<CorruptedTomeDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<CorruptedTome>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_HeartOfOak_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<HeartOfOakDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<HeartOfOak>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_IronHeart_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<IronHeartDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<IronHeart>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_Quickboots_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<QuickbootsDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<Quickboots>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_SteelScales_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<SteelScalesDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<SteelScales>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_ThornArmor_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<ThornArmorDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<ThornArmor>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_TwinBlades_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<TwinBladesDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<TwinBlades>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_VampiricFang_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<VampiricFangDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<VampiricFang>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_WarGauntlet_AddsArtifactToUnit()
        {
            var artifact = ScriptableObject.CreateInstance<WarGauntletDefinition>();
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<WarGauntlet>(_unit.Artifacts[0]);
        }

        // ---- MULTIPLE ARTIFACTS ----

        [Test]
        public void MultipleArtifacts_AccumulateEffects()
        {
            var phantom = ScriptableObject.CreateInstance<PhantomStrikeDefinition>();
            var shield = ScriptableObject.CreateInstance<DeathShieldDefinition>();

            ArtifactApplier.ApplyToPlayer(phantom, _unit);
            ArtifactApplier.ApplyToPlayer(shield, _unit);

            Assert.AreEqual(2, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Artifacts[0]);
            Assert.IsInstanceOf<DeathShield>(_unit.Artifacts[1]);
        }
    }
}
