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

        private ArtifactDefinition CreateArtifactPassive(string id)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit(id, "Test", "desc", Rarity.Common, ArtifactTag.None,
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
            var artifact = CreateArtifactPassive("artifact_crown_of_echoes");
            Assert.Throws<ArgumentNullException>(() => ArtifactApplier.ApplyToPlayer(artifact, null));
        }

        // ---- ARTIFACT PASSIVES ----

        [Test]
        public void Artifact_PhantomStrike_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("artifact_crown_of_echoes");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_DeathShield_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("artifact_hourglass");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<DeathShield>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_CritChance_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("artifact_lucky_horseshoe");
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<CritChancePassive>(_unit.Passives[0]);
        }

        [Test]
        public void Artifact_PoisonAmplifier_AddsPassiveToUnit()
        {
            var artifact = CreateArtifactPassive("artifact_poison_darts");
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
            var phantom = CreateArtifactPassive("artifact_crown_of_echoes");
            var shield = CreateArtifactPassive("artifact_hourglass");

            ArtifactApplier.ApplyToPlayer(phantom, _unit);
            ArtifactApplier.ApplyToPlayer(shield, _unit);

            Assert.AreEqual(2, _unit.Passives.Count);
            Assert.IsInstanceOf<PhantomStrike>(_unit.Passives[0]);
            Assert.IsInstanceOf<DeathShield>(_unit.Passives[1]);
        }
    }
}
