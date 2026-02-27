using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    internal class MockArtifactRepository : IArtifactRepository
    {
        private readonly List<ArtifactDefinition> _artifacts;

        public MockArtifactRepository(List<ArtifactDefinition> artifacts)
        {
            _artifacts = artifacts;
        }

        public IReadOnlyList<ArtifactDefinition> GetAll() => _artifacts;
    }

    internal class FixedArtifactSelector : IArtifactSelector
    {
        private readonly ArtifactDefinition[] _fixed;

        public FixedArtifactSelector(params ArtifactDefinition[] artifacts)
        {
            _fixed = artifacts;
        }

        public ArtifactDefinition[] Select(IReadOnlyList<ArtifactDefinition> pool, int bossId, int count)
        {
            return _fixed;
        }
    }

    public class BossDropSystemTests
    {
        private ArtifactMetaProgression _meta;

        [SetUp]
        public void Setup()
        {
            var savePath = Path.Combine(Path.GetTempPath(), $"artifact_test_{System.Guid.NewGuid()}.json");
            _meta = new ArtifactMetaProgression(new JsonArtifactProgressionPersistence(savePath));
        }

        [TearDown]
        public void Teardown()
        {
            _meta.Reset();
        }

        private ArtifactDefinition CreateArtifact(string id, bool lockedByDefault = false)
        {
            var a = ScriptableObject.CreateInstance<ArtifactDefinition>();
            a.EditorInit(id, id, "desc", Rarity.Common, ArtifactTag.None,
                ArtifactEffectType.AddArtifact, StatType.Armor, 10, "", lockedByDefault);
            return a;
        }

        [Test]
        public void GetBossDrop_ReturnsMaxThreeArtifacts()
        {
            var artifacts = new List<ArtifactDefinition>();
            for (var i = 0; i < 5; i++)
            {
                var a = CreateArtifact($"artifact_{i}");
                artifacts.Add(a);
                _meta.Unlock($"artifact_{i}");
            }

            var repo = new MockArtifactRepository(artifacts);
            var system = new BossDropSystem(repo, _meta);

            var drop = system.GetBossDrop(1);

            Assert.LessOrEqual(drop.Length, 3);
        }

        [Test]
        public void GetBossDrop_OnlyReturnsUnlockedArtifacts()
        {
            var locked1 = CreateArtifact("locked_1", true);
            var locked2 = CreateArtifact("locked_2", true);
            var unlocked1 = CreateArtifact("unlocked_1");
            var unlocked2 = CreateArtifact("unlocked_2");
            var unlocked3 = CreateArtifact("unlocked_3");

            _meta.Unlock("unlocked_1");
            _meta.Unlock("unlocked_2");
            _meta.Unlock("unlocked_3");

            var repo = new MockArtifactRepository(new List<ArtifactDefinition>
                { locked1, locked2, unlocked1, unlocked2, unlocked3 });
            var system = new BossDropSystem(repo, _meta);

            var drop = system.GetBossDrop(1);

            foreach (var artifact in drop)
            {
                Assert.IsTrue(_meta.IsUnlocked(artifact.Id),
                    $"Artifact '{artifact.Id}' should be unlocked");
            }
        }

        [Test]
        public void GetBossDrop_ReturnsEmpty_WhenNoUnlockedArtifacts()
        {
            var locked1 = CreateArtifact("locked_1", true);
            var locked2 = CreateArtifact("locked_2", true);

            var repo = new MockArtifactRepository(new List<ArtifactDefinition> { locked1, locked2 });
            var system = new BossDropSystem(repo, _meta);

            var drop = system.GetBossDrop(1);

            Assert.AreEqual(0, drop.Length);
        }

        [Test]
        public void GetBossDrop_ReturnsFewerThanThree_WhenPoolIsSmall()
        {
            var a1 = CreateArtifact("a1");
            var a2 = CreateArtifact("a2");
            _meta.Unlock("a1");
            _meta.Unlock("a2");

            var repo = new MockArtifactRepository(new List<ArtifactDefinition> { a1, a2 });
            var system = new BossDropSystem(repo, _meta);

            var drop = system.GetBossDrop(1);

            Assert.AreEqual(2, drop.Length);
        }

        [Test]
        public void GetBossDrop_UsesSelectorForSelection()
        {
            var a1 = CreateArtifact("a1");
            var a2 = CreateArtifact("a2");
            _meta.Unlock("a1");
            _meta.Unlock("a2");

            var repo = new MockArtifactRepository(new List<ArtifactDefinition> { a1, a2 });
            var fixedSelector = new FixedArtifactSelector(a1);
            var system = new BossDropSystem(repo, _meta, fixedSelector);

            var drop = system.GetBossDrop(1);

            Assert.AreEqual(1, drop.Length);
            Assert.AreEqual("a1", drop[0].Id);
        }

        [Test]
        public void GetBossDrop_NullRepository_ThrowsOnConstruction()
        {
            Assert.Throws<System.ArgumentNullException>(() => new BossDropSystem(null, _meta));
        }

        [Test]
        public void GetBossDrop_NullMetaProgression_ThrowsOnConstruction()
        {
            var repo = new MockArtifactRepository(new List<ArtifactDefinition>());
            Assert.Throws<System.ArgumentNullException>(() => new BossDropSystem(repo, null));
        }

        [Test]
        public void GetBossDrop_NullSelector_ThrowsOnConstruction()
        {
            var repo = new MockArtifactRepository(new List<ArtifactDefinition>());
            Assert.Throws<System.ArgumentNullException>(() => new BossDropSystem(repo, _meta, null));
        }

        [Test]
        public void GetBossDrop_MultipleCallsCanReturnDifferentResults()
        {
            var artifacts = new List<ArtifactDefinition>();
            for (var i = 0; i < 10; i++)
            {
                var a = CreateArtifact($"artifact_{i}");
                artifacts.Add(a);
                _meta.Unlock($"artifact_{i}");
            }

            var repo = new MockArtifactRepository(artifacts);
            var system = new BossDropSystem(repo, _meta);

            var drop1 = system.GetBossDrop(1);
            var drop2 = system.GetBossDrop(2);

            Assert.AreEqual(3, drop1.Length);
            Assert.AreEqual(3, drop2.Length);
        }

        [Test]
        public void GetBossDrop_WithTwelveArtifacts_ReturnsExactlyThree()
        {
            var artifacts = new List<ArtifactDefinition>();
            var ids = new[]
            {
                "artifact_iron_heart", "artifact_war_gauntlet", "artifact_steel_scales",
                "artifact_quickboots", "artifact_vampiric_fang", "artifact_blazing_torch",
                "artifact_arcane_tome", "artifact_poisoned_blade", "artifact_berserker_mask",
                "artifact_thorn_armor", "artifact_twin_blades", "artifact_blood_ritual"
            };

            foreach (var id in ids)
            {
                var a = CreateArtifact(id);
                artifacts.Add(a);
                _meta.Unlock(id);
            }

            var repo = new MockArtifactRepository(artifacts);
            var system = new BossDropSystem(repo, _meta);

            var drop = system.GetBossDrop(1);

            Assert.AreEqual(3, drop.Length);
        }
    }
}
