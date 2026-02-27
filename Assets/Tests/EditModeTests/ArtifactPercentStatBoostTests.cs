using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class ArtifactPercentStatBoostTests
    {
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            _unit = new Unit("Hero")
            {
                Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 20, Armor = 10, Speed = 5 }
            };
        }

        private ArtifactDefinition CreatePercentArtifact(StatType stat, int percent)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit("test", "Test", "desc", Rarity.Rare, ArtifactTag.None,
                ArtifactEffectType.PercentStatBoost, stat, percent, "", false);
            return a;
        }

        [Test]
        public void PercentStatBoost_Armor50_IncreasesArmorByHalf()
        {
            var artifact = CreatePercentArtifact(StatType.Armor, 50);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(15, _unit.Stats.Armor, "10 armor * 150% = 15");
        }

        [Test]
        public void PercentStatBoost_HP100_DoublesMaxHPAndCurrentHP()
        {
            var artifact = CreatePercentArtifact(StatType.MaxHP, 100);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(200, _unit.Stats.MaxHP);
            Assert.AreEqual(200, _unit.Stats.CurrentHP);
        }

        [Test]
        public void PercentStatBoost_AttackPower50_IncreasesAttackByHalf()
        {
            var artifact = CreatePercentArtifact(StatType.AttackPower, 50);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(30, _unit.Stats.AttackPower, "20 attack * 150% = 30");
        }

        [Test]
        public void PercentStatBoost_Speed100_DoublesSpeed()
        {
            var artifact = CreatePercentArtifact(StatType.Speed, 100);
            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(10, _unit.Stats.Speed, "5 speed * 200% = 10");
        }

        [Test]
        public void PercentStatBoost_UnknownStat_Throws()
        {
            var artifact = CreatePercentArtifact((StatType)999, 50);
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                ArtifactApplier.ApplyToPlayer(artifact, _unit));
        }

        [Test]
        public void HeartOfOak_ViaArtifactApplier_GivesPercent50ArmorBoost()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("artifact_heart_of_oak", "Heart of the Oak", "Increase armor by 50%.",
                Rarity.Rare, ArtifactTag.None, ArtifactEffectType.PercentStatBoost, StatType.Armor, 50, "", true);

            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(15, _unit.Stats.Armor, "10 * 150% = 15");
        }
    }
}
