using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class BossManagerTests
    {
        private BossRegistry CreateRegistry(params BossDefinition[] bosses)
        {
            var registry = ScriptableObject.CreateInstance<BossRegistry>();
            foreach (var boss in bosses)
                registry.EditorAddBoss(boss);
            return registry;
        }

        private BossDefinition CreateBoss(string id, int difficultyRating)
        {
            var boss = ScriptableObject.CreateInstance<BossDefinition>();
            boss.EditorInit(id, id, difficultyRating);
            return boss;
        }

        // ── IsBossFight ──────────────────────────────────────────────────────────

        [Test]
        public void IsBossFight_ReturnsFalse_ForFightIndexZero()
        {
            var registry = CreateRegistry(CreateBoss("boss1", 1));
            var manager = new BossManager(registry);

            Assert.IsFalse(manager.IsBossFight(0));
        }

        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(100)]
        public void IsBossFight_ReturnsTrue_ForMultiplesOfTen(int fightIndex)
        {
            var registry = CreateRegistry(CreateBoss("boss1", 1));
            var manager = new BossManager(registry);

            Assert.IsTrue(manager.IsBossFight(fightIndex));
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(9)]
        [TestCase(11)]
        [TestCase(19)]
        public void IsBossFight_ReturnsFalse_ForNonMultiplesOfTen(int fightIndex)
        {
            var registry = CreateRegistry(CreateBoss("boss1", 1));
            var manager = new BossManager(registry);

            Assert.IsFalse(manager.IsBossFight(fightIndex));
        }

        // ── GetBoss ───────────────────────────────────────────────────────────────

        [Test]
        public void GetBoss_ReturnsNull_WhenRegistryIsEmpty()
        {
            var registry = CreateRegistry();
            var manager = new BossManager(registry);

            var result = manager.GetBoss(10);

            Assert.IsNull(result);
        }

        [Test]
        public void GetBoss_ReturnsBoss_WhenSingleBossMatchesTier()
        {
            var boss = CreateBoss("boss1", 1);
            var registry = CreateRegistry(boss);
            var manager = new BossManager(registry);

            var result = manager.GetBoss(10); // tier = 1

            Assert.AreEqual(boss, result);
        }

        [Test]
        public void GetBoss_ReturnsLowestRatedBoss_WhenNoCandidatesMatchTier()
        {
            var hardBoss = CreateBoss("hard_boss", 5);
            var registry = CreateRegistry(hardBoss);
            var manager = new BossManager(registry);

            // tier = 1, but boss requires tier 5 → falls back
            var result = manager.GetBoss(10);

            Assert.AreEqual(hardBoss, result);
        }

        [Test]
        public void GetBoss_OnlyReturnsEligibleBosses_BasedOnDifficultyRating()
        {
            var easyBoss = CreateBoss("easy_boss", 1);
            var hardBoss = CreateBoss("hard_boss", 5);
            var registry = CreateRegistry(easyBoss, hardBoss);
            var manager = new BossManager(registry);

            // At tier 1 only easyBoss is eligible
            for (var i = 0; i < 20; i++)
            {
                var result = manager.GetBoss(10); // tier = 1
                Assert.AreEqual("easy_boss", result.Id,
                    "Hard boss should not be selected when its DifficultyRating exceeds the current tier");
            }
        }

        [Test]
        public void GetBoss_CanReturnHardBoss_OnceTierReached()
        {
            var hardBoss = CreateBoss("hard_boss", 5);
            var registry = CreateRegistry(hardBoss);
            var manager = new BossManager(registry);

            var result = manager.GetBoss(50); // tier = 5

            Assert.AreEqual(hardBoss, result);
        }

        [Test]
        public void BossManager_Constructor_ThrowsOnNullRegistry()
        {
            Assert.Throws<System.ArgumentNullException>(() => _ = new BossManager(null));
        }
    }
}
