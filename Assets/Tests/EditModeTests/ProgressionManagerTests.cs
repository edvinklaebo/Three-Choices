using System.Collections.Generic;

using Core.Progression;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class ProgressionManagerTests
    {
        private ProgressionConfig _config;
        private ProgressionManager _manager;
        private string _saveKey;

        [SetUp]
        public void Setup()
        {
            _saveKey = $"progression_test_{System.Guid.NewGuid():N}";
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();
            _config.GenerateExampleUnlocks();
            _manager = new ProgressionManager(_config, _saveKey);
            _manager.ResetProgress();
        }

        [TearDown]
        public void Teardown()
        {
            _manager.ResetProgress();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void OnEnemyKilled_AddsPoints()
        {
            _manager.OnEnemyKilled(5);
            Assert.AreEqual(5, _manager.TotalPoints);
        }

        [Test]
        public void OnEnemyKilled_UnlocksEligibleEntries_AndRaisesEvent()
        {
            ProgressionUnlockDefinition unlocked = null;
            _manager.UnlockAchieved += unlock => unlocked = unlock;

            _manager.OnEnemyKilled(10);

            Assert.IsNotNull(unlocked);
            Assert.AreEqual("character_warrior", unlocked.Id);
            Assert.IsTrue(_manager.IsUnlocked("character_warrior"));
        }

        [Test]
        public void Unlock_WithMissingDependency_DoesNotUnlock()
        {
            var customConfig = ScriptableObject.CreateInstance<ProgressionConfig>();
            var unlocks = new List<ProgressionUnlockDefinition>
            {
                CreateUnlock("u1", 5, ProgressionUnlockCategory.Character),
                CreateUnlock("u2", 10, ProgressionUnlockCategory.Artifact, "missing_dependency")
            };

            customConfig.SetUnlocks(unlocks, expectedUnlockCount: 2, targetPoints: 50);
            var manager = new ProgressionManager(customConfig, _saveKey + "_deps");
            manager.ResetProgress();

            manager.OnEnemyKilled(20);

            Assert.IsTrue(manager.IsUnlocked("u1"));
            Assert.IsFalse(manager.IsUnlocked("u2"));

            manager.ResetProgress();
            Object.DestroyImmediate(customConfig);
        }

        [Test]
        public void SaveAndLoad_PersistsPointsAndUnlockedIds()
        {
            _manager.OnEnemyKilled(120);

            var loadedManager = new ProgressionManager(_config, _saveKey);
            loadedManager.LoadProgress();

            Assert.AreEqual(120, loadedManager.TotalPoints);
            Assert.IsTrue(loadedManager.IsUnlocked("character_warrior"));
            Assert.IsTrue(loadedManager.IsUnlocked("passive_toughness"));
            Assert.IsTrue(loadedManager.IsUnlocked("ability_firebolt"));

            loadedManager.ResetProgress();
        }

        [Test]
        public void Config_GenerateCurvedProgression_Creates100Unlocks_AndFinalCostNearTarget()
        {
            _config.GenerateCurvedProgression(100, 10000);

            Assert.AreEqual(100, _config.Unlocks.Count);
            Assert.AreEqual(10000, _config.Unlocks[99].UnlockCost);
            Assert.IsTrue(_config.IsValid());

            var previousCost = -1;
            for (var i = 0; i < _config.Unlocks.Count; i++)
            {
                var current = _config.Unlocks[i].UnlockCost;
                Assert.Greater(current, previousCost, "Unlock costs must be strictly increasing");
                previousCost = current;
            }
        }

        [Test]
        public void Config_GenerateExampleUnlocks_CreatesAtLeast10Unlocks()
        {
            _config.GenerateExampleUnlocks();
            Assert.GreaterOrEqual(_config.Unlocks.Count, 10);
        }

        private static ProgressionUnlockDefinition CreateUnlock(string id, int cost,
                                                                ProgressionUnlockCategory category,
                                                                string dependency = null)
        {
            var unlock = new ProgressionUnlockDefinition();
            var dependencies = string.IsNullOrEmpty(dependency)
                ? new List<string>()
                : new List<string> { dependency };

            unlock.Initialize(id, id, id, cost, category, dependencies);
            return unlock;
        }
    }
}
