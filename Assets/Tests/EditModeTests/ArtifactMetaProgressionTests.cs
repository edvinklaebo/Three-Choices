using System.IO;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class ArtifactMetaProgressionTests
    {
        private ArtifactMetaProgression _meta;
        private string _savePath;

        [SetUp]
        public void Setup()
        {
            _savePath = Path.Combine(Path.GetTempPath(), $"artifact_meta_test_{System.Guid.NewGuid()}.json");
            _meta = new ArtifactMetaProgression(_savePath);
        }

        [TearDown]
        public void Teardown()
        {
            _meta.Reset();
        }

        [Test]
        public void IsUnlocked_ReturnsFalse_ForUnknownArtifact()
        {
            Assert.IsFalse(_meta.IsUnlocked("nonexistent_artifact"));
        }

        [Test]
        public void Unlock_AddsArtifactToUnlockedSet()
        {
            _meta.Unlock("artifact_iron_heart");
            Assert.IsTrue(_meta.IsUnlocked("artifact_iron_heart"));
        }

        [Test]
        public void Unlock_SameIdTwice_StaysUnlocked()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Unlock("artifact_iron_heart");
            Assert.IsTrue(_meta.IsUnlocked("artifact_iron_heart"));
        }
        
        [Test]
        public void Lock_RemovesArtifactFromUnlockedSet()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Lock("artifact_iron_heart");
            Assert.IsFalse(_meta.IsUnlocked("artifact_iron_heart"));
        }

        [Test]
        public void Lock_NonExistentId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _meta.Lock("nonexistent_artifact"));
        }

        [Test]
        public void GetUnlockedIds_ReturnsAllUnlockedArtifacts()
        {
            _meta.Unlock("a1");
            _meta.Unlock("a2");
            _meta.Unlock("a3");

            var ids = _meta.GetUnlockedIds();

            Assert.AreEqual(3, ids.Count);
            Assert.IsTrue(((System.Collections.Generic.ICollection<string>)ids).Contains("a1"));
            Assert.IsTrue(((System.Collections.Generic.ICollection<string>)ids).Contains("a2"));
            Assert.IsTrue(((System.Collections.Generic.ICollection<string>)ids).Contains("a3"));
        }

        [Test]
        public void GetUnlockedIds_ReturnsEmpty_WhenNothingUnlocked()
        {
            var ids = _meta.GetUnlockedIds();
            Assert.AreEqual(0, ids.Count);
        }

        [Test]
        public void SaveAndLoad_RoundtripPreservesUnlockedIds()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Unlock("artifact_war_gauntlet");
            _meta.Save();

            var loaded = new ArtifactMetaProgression(_savePath);
            loaded.Load();

            Assert.IsTrue(loaded.IsUnlocked("artifact_iron_heart"));
            Assert.IsTrue(loaded.IsUnlocked("artifact_war_gauntlet"));
            Assert.AreEqual(2, loaded.GetUnlockedIds().Count);
        }

        [Test]
        public void Load_ReturnsFalse_WhenNoSaveFile()
        {
            var result = _meta.Load();
            Assert.IsFalse(result);
        }

        [Test]
        public void Load_ReturnsTrue_WhenSaveFileExists()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Save();

            var loaded = new ArtifactMetaProgression(_savePath);
            var result = loaded.Load();

            Assert.IsTrue(result);
        }

        [Test]
        public void Reset_ClearsAllUnlockedArtifacts()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Unlock("artifact_war_gauntlet");
            _meta.Reset();

            Assert.IsFalse(_meta.IsUnlocked("artifact_iron_heart"));
            Assert.IsFalse(_meta.IsUnlocked("artifact_war_gauntlet"));
            Assert.AreEqual(0, _meta.GetUnlockedIds().Count);
        }

        [Test]
        public void Reset_DeletesSaveFile()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Save();

            Assert.IsTrue(File.Exists(_savePath));

            _meta.Reset();

            Assert.IsFalse(File.Exists(_savePath));
        }

        [Test]
        public void Unlock_AutoSaves_ToDisk()
        {
            _meta.Unlock("artifact_iron_heart");

            Assert.IsTrue(File.Exists(_savePath),
                "Unlock should auto-save to disk");

            var loaded = new ArtifactMetaProgression(_savePath);
            loaded.Load();
            Assert.IsTrue(loaded.IsUnlocked("artifact_iron_heart"));
        }

        [Test]
        public void SaveAndLoad_EmptySet_RoundtripWorks()
        {
            _meta.Save();

            var loaded = new ArtifactMetaProgression(_savePath);
            loaded.Load();

            Assert.AreEqual(0, loaded.GetUnlockedIds().Count);
        }

        [Test]
        public void LoadThenUnlock_WorksCorrectly()
        {
            _meta.Unlock("artifact_iron_heart");
            _meta.Save();

            var loaded = new ArtifactMetaProgression(_savePath);
            loaded.Load();
            loaded.Unlock("artifact_war_gauntlet");

            Assert.IsTrue(loaded.IsUnlocked("artifact_iron_heart"));
            Assert.IsTrue(loaded.IsUnlocked("artifact_war_gauntlet"));
        }

        [Test]
        public void UnlockAllTwelveArtifacts_WorksCorrectly()
        {
            var ids = new[]
            {
                "artifact_iron_heart", "artifact_war_gauntlet", "artifact_steel_scales",
                "artifact_quickboots", "artifact_vampiric_fang", "artifact_blazing_torch",
                "artifact_arcane_tome", "artifact_poisoned_blade", "artifact_berserker_mask",
                "artifact_thorn_armor", "artifact_twin_blades", "artifact_blood_ritual"
            };

            foreach (var id in ids)
                _meta.Unlock(id);

            Assert.AreEqual(12, _meta.GetUnlockedIds().Count);

            foreach (var id in ids)
                Assert.IsTrue(_meta.IsUnlocked(id), $"Artifact '{id}' should be unlocked");
        }
    }
}
