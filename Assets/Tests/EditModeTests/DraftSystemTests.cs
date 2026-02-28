using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class MockUpgradeRepository : IUpgradeRepository
    {
        private readonly List<UpgradeDefinition> _upgrades;

        public MockUpgradeRepository(List<UpgradeDefinition> upgrades)
        {
            _upgrades = upgrades;
        }

        public List<UpgradeDefinition> GetAll()
        {
            return _upgrades;
        }
    }

    public class MockArtifactRepository : IArtifactRepository
    {
        private readonly IReadOnlyList<ArtifactDefinition> _artifacts;

        public MockArtifactRepository(List<ArtifactDefinition> artifacts)
        {
            _artifacts = artifacts;
        }

        public IReadOnlyList<ArtifactDefinition> GetAll() => _artifacts;
    }

    public class MockRarityRoller : IRarityRoller
    {
        private readonly Queue<Rarity> _rarities;

        public MockRarityRoller(params Rarity[] rarities)
        {
            _rarities = new Queue<Rarity>(rarities);
        }

        public Rarity RollRarity()
        {
            return _rarities.Count > 0 ? _rarities.Dequeue() : Rarity.Common;
        }
    }

    public class DraftSystemTests
    {
        private UpgradeDefinition CreateUpgrade(string name, int rarityWeight = 100)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit(name, name);
            // Use reflection to set rarityWeight since there's no public setter
            var field = typeof(UpgradeDefinition).GetField("rarityWeight",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(upgrade, rarityWeight);
            return upgrade;
        }

        private ArtifactDefinition CreateArtifact(string id, string name, Rarity rarity = Rarity.Common)
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(id, name, string.Empty, rarity, ArtifactTag.None, ArtifactEffectType.AddArtifact,
                false);
            return artifact;
        }

        [Test]
        public void GenerateDraft_ReturnsCorrectNumberOfUpgrades()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("A"),
                CreateUpgrade("B"),
                CreateUpgrade("C")
            };

            var repository = new MockUpgradeRepository(upgrades);
            var draftSystem = new DraftSystem(repository);

            var draft = draftSystem.GenerateDraft(2);

            Assert.AreEqual(2, draft.Count);
            Assert.AreEqual(2, draft.Distinct().Count());
        }

        [Test]
        public void GenerateDraft_DoesNotReturnMoreThanPool()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("A"),
                CreateUpgrade("B")
            };

            var repository = new MockUpgradeRepository(upgrades);
            var draftSystem = new DraftSystem(repository);

            var draft = draftSystem.GenerateDraft(5);

            Assert.AreEqual(2, draft.Count);
        }

        [Test]
        public void GenerateDraft_SelectsUpgradeOfRolledRarity()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("Common1"), // Common
                CreateUpgrade("Uncommon1", 50), // Uncommon
                CreateUpgrade("Rare1", 25), // Rare
                CreateUpgrade("Epic1", 10) // Epic
            };

            var repository = new MockUpgradeRepository(upgrades);
            var rarityRoller = new MockRarityRoller(Rarity.Rare);
            var draftSystem = new DraftSystem(repository, rarityRoller);

            var draft = draftSystem.GenerateDraft(1);

            Assert.AreEqual(1, draft.Count);
            Assert.AreEqual("Rare1", draft[0].DisplayName);
        }

        [Test]
        public void GenerateDraft_FallbackToLowerRarityWhenTargetNotAvailable()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("Common1"), // Common
                CreateUpgrade("Common2") // Common
            };

            var repository = new MockUpgradeRepository(upgrades);
            var rarityRoller = new MockRarityRoller(Rarity.Epic); // Roll Epic but only Common available
            var draftSystem = new DraftSystem(repository, rarityRoller);

            var draft = draftSystem.GenerateDraft(1);

            Assert.AreEqual(1, draft.Count);
            Assert.AreEqual(Rarity.Common, draft[0].GetRarity());
        }

        [Test]
        public void GenerateDraft_DoesNotSelectSameUpgradeTwice()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("Common1"),
                CreateUpgrade("Common2"),
                CreateUpgrade("Common3")
            };

            var repository = new MockUpgradeRepository(upgrades);
            var rarityRoller = new MockRarityRoller(Rarity.Common, Rarity.Common, Rarity.Common);
            var draftSystem = new DraftSystem(repository, rarityRoller);

            var draft = draftSystem.GenerateDraft(3);

            Assert.AreEqual(3, draft.Count);
            Assert.AreEqual(3, draft.Distinct().Count());
        }

        [Test]
        public void GenerateDraft_WithMixedRarities()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("Epic1", 10),
                CreateUpgrade("Rare1", 25),
                CreateUpgrade("Uncommon1", 50),
                CreateUpgrade("Common1")
            };

            var repository = new MockUpgradeRepository(upgrades);
            var rarityRoller = new MockRarityRoller(Rarity.Epic, Rarity.Rare, Rarity.Uncommon);
            var draftSystem = new DraftSystem(repository, rarityRoller);

            var draft = draftSystem.GenerateDraft(3);

            Assert.AreEqual(3, draft.Count);
            Assert.AreEqual("Epic1", draft[0].DisplayName);
            Assert.AreEqual("Rare1", draft[1].DisplayName);
            Assert.AreEqual("Uncommon1", draft[2].DisplayName);
        }

        [Test]
        public void GenerateDraft_IncludesArtifactsWhenRepositoryProvided()
        {
            var upgrades = new List<UpgradeDefinition> { CreateUpgrade("UpgradeA") };
            var artifacts = new List<ArtifactDefinition>
            {
                CreateArtifact("artifact_1", "ArtifactA")
            };

            var upgradeRepo = new MockUpgradeRepository(upgrades);
            var artifactRepo = new MockArtifactRepository(artifacts);
            var draftSystem = new DraftSystem(upgradeRepo, artifactRepo);

            var draft = draftSystem.GenerateDraft(2);

            Assert.AreEqual(2, draft.Count);
            Assert.IsTrue(draft.Any(o => o.DisplayName == "UpgradeA" && !o.IsArtifact));
            Assert.IsTrue(draft.Any(o => o.DisplayName == "ArtifactA" && o.IsArtifact));
        }

        [Test]
        public void GenerateDraft_ArtifactOption_IsArtifactIsTrue()
        {
            var upgrades = new List<UpgradeDefinition>();
            var artifacts = new List<ArtifactDefinition>
            {
                CreateArtifact("artifact_test", "TestArtifact", Rarity.Rare)
            };

            var upgradeRepo = new MockUpgradeRepository(upgrades);
            var artifactRepo = new MockArtifactRepository(artifacts);
            var rarityRoller = new MockRarityRoller(Rarity.Rare);
            var draftSystem = new DraftSystem(upgradeRepo, artifactRepo, rarityRoller);

            var draft = draftSystem.GenerateDraft(1);

            Assert.AreEqual(1, draft.Count);
            Assert.IsTrue(draft[0].IsArtifact);
            Assert.AreEqual("TestArtifact", draft[0].DisplayName);
            Assert.AreEqual(Rarity.Rare, draft[0].GetRarity());
        }

        [Test]
        public void GenerateDraft_WithoutArtifactRepository_OnlyIncludesUpgrades()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                CreateUpgrade("UpgradeA"),
                CreateUpgrade("UpgradeB")
            };

            var repository = new MockUpgradeRepository(upgrades);
            var draftSystem = new DraftSystem(repository);

            var draft = draftSystem.GenerateDraft(2);

            Assert.AreEqual(2, draft.Count);
            Assert.IsTrue(draft.All(o => !o.IsArtifact));
        }
    }
}