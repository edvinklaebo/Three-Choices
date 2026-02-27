using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class ArtifactDefinitionTests
    {
        private ArtifactDefinition CreateArtifact(
            string id = "test_artifact",
            string displayName = "Test Artifact",
            string description = "A test artifact.",
            Rarity rarity = Rarity.Common,
            ArtifactTag tags = ArtifactTag.None,
            ArtifactEffectType effectType = ArtifactEffectType.AddArtifact,
            StatType stat = StatType.MaxHP,
            int amount = 10,
            string abilityId = "",
            bool lockedByDefault = false)
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(id, displayName, description, rarity, tags, effectType, stat, amount, abilityId,
                lockedByDefault);
            return artifact;
        }

        [Test]
        public void Id_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(id: "artifact_iron_heart");
            Assert.AreEqual("artifact_iron_heart", artifact.Id);
        }

        [Test]
        public void DisplayName_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(displayName: "Iron Heart");
            Assert.AreEqual("Iron Heart", artifact.DisplayName);
        }

        [Test]
        public void Description_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(description: "Increases max HP by 20.");
            Assert.AreEqual("Increases max HP by 20.", artifact.Description);
        }

        [Test]
        public void Rarity_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(rarity: Rarity.Rare);
            Assert.AreEqual(Rarity.Rare, artifact.Rarity);
        }

        [Test]
        public void Tags_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(tags: ArtifactTag.Lifesteal | ArtifactTag.Fireball);
            Assert.AreEqual(ArtifactTag.Lifesteal | ArtifactTag.Fireball, artifact.Tags);
        }

        [Test]
        public void Tags_HasFlagWorksCorrectly()
        {
            var artifact = CreateArtifact(tags: ArtifactTag.Lifesteal | ArtifactTag.DoubleStrike);
            Assert.IsTrue(artifact.Tags.HasFlag(ArtifactTag.Lifesteal));
            Assert.IsTrue(artifact.Tags.HasFlag(ArtifactTag.DoubleStrike));
            Assert.IsFalse(artifact.Tags.HasFlag(ArtifactTag.Fireball));
        }

        [Test]
        public void EffectType_AddArtifact_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(effectType: ArtifactEffectType.AddArtifact);
            Assert.AreEqual(ArtifactEffectType.AddArtifact, artifact.EffectType);
        }

        [Test]
        public void EffectType_AddAbility_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(effectType: ArtifactEffectType.AddAbility);
            Assert.AreEqual(ArtifactEffectType.AddAbility, artifact.EffectType);
        }

        [Test]
        public void Stat_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(stat: StatType.AttackPower);
            Assert.AreEqual(StatType.AttackPower, artifact.Stat);
        }

        [Test]
        public void Amount_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(amount: 25);
            Assert.AreEqual(25, artifact.Amount);
        }

        [Test]
        public void AbilityId_ReturnsCorrectValue()
        {
            var artifact = CreateArtifact(abilityId: "Fireball");
            Assert.AreEqual("Fireball", artifact.AbilityId);
        }

        [Test]
        public void LockedByDefault_True_WhenSetToTrue()
        {
            var artifact = CreateArtifact(lockedByDefault: true);
            Assert.IsTrue(artifact.LockedByDefault);
        }

        [Test]
        public void LockedByDefault_False_WhenSetToFalse()
        {
            var artifact = CreateArtifact(lockedByDefault: false);
            Assert.IsFalse(artifact.LockedByDefault);
        }

        [Test]
        public void AllTwelveArtifactTags_AreMutuallyExclusive()
        {
            var allTags = new[]
            {
                ArtifactTag.Fireball, ArtifactTag.ArcaneMissiles, ArtifactTag.DoubleStrike,
                ArtifactTag.Lifesteal, ArtifactTag.Poison, ArtifactTag.Bleed,
                ArtifactTag.Thorns, ArtifactTag.Rage
            };

            for (var i = 0; i < allTags.Length; i++)
            for (var j = i + 1; j < allTags.Length; j++)
                Assert.AreEqual(0, (int)allTags[i] & (int)allTags[j],
                    $"Tags {allTags[i]} and {allTags[j]} should not overlap");
        }
    }
}
