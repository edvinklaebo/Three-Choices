using System;

using Core;
using Core.Artifacts;
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

        private ArtifactDefinition CreateArtifactPassive(ArtifactId artifactId)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit(artifactId, "Test", "desc", Rarity.Common, ArtifactTag.None,
                ArtifactEffectType.AddArtifact, false);
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
            var artifact = CreateArtifactPassive(ArtifactId.CrownOfEchoes);
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(artifact, null));
        }

        // ---- ARTIFACT PASSIVES ----

        [Test]
        public void Artifact_PhantomStrike_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.CrownOfEchoes);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_DeathShield_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.Hourglass);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<DeathShield>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_CritChance_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.LuckyHorseshoe);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<CritChance>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_PoisonAmplifier_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.PoisonDarts);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PoisonAmplifier>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BerserkerMask_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.BerserkerMask);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BerserkerMask>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BlazingTorch_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.BlazingTorch);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BlazingTorch>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_BloodRitual_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.BloodRitual);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<BloodRitual>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_CorruptedTome_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.CorruptedTome);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<CorruptedTome>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_HeartOfOak_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.HeartOfOak);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<HeartOfOak>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_IronHeart_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.IronHeart);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<IronHeart>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_Quickboots_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.Quickboots);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<Quickboots>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_SteelScales_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.SteelScales);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<SteelScales>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_ThornArmor_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.ThornArmor);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<ThornArmor>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_TwinBlades_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.TwinBlades);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<TwinBlades>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_VampiricFang_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.VampiricFang);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<VampiricFang>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_WarGauntlet_AddsArtifactToUnit()
        {
            var artifact = CreateArtifactPassive(ArtifactId.WarGauntlet);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Artifacts.Count);
            Assert.IsInstanceOf<WarGauntlet>(_unit.Artifacts[0]);
        }

        [Test]
        public void Artifact_Unknown_Throws()
        {
            var artifact = CreateArtifactPassive((ArtifactId)999);
            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        [Test]
        public void UnknownEffectType_Throws()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Test", "desc", Rarity.Common, ArtifactTag.None,
                (ArtifactEffectType)999,  false);

            Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        // ---- MULTIPLE ARTIFACTS ----

        [Test]
        public void MultipleArtifacts_AccumulateEffects()
        {
            var phantom = CreateArtifactPassive(ArtifactId.CrownOfEchoes);
            var shield = CreateArtifactPassive(ArtifactId.Hourglass);

            ArtifactApplier.ApplyToPlayer(phantom, _unit);
            ArtifactApplier.ApplyToPlayer(shield, _unit);

            Assert.AreEqual(2, _unit.Artifacts.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Artifacts[0]);
            Assert.IsInstanceOf<DeathShield>(_unit.Artifacts[1]);
        }
    }
}
