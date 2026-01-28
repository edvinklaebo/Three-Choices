using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

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

    public class DraftSystemTests
    {
        [Test]
        public void GenerateDraft_ReturnsCorrectNumberOfUpgrades()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                new() {DisplayName = "A"},
                new() {DisplayName = "B"},
                new() {DisplayName = "C"}
            };

            var repository = new MockUpgradeRepository(upgrades);
            var draftSystem = new DraftSystem(repository);

            var draft = draftSystem.GenerateDraft(2);

            Assert.AreEqual(2, draft.Count);
            Assert.IsTrue(draft.All(u => upgrades.Contains(u)));
            Assert.AreEqual(2, draft.Distinct().Count());
        }

        [Test]
        public void GenerateDraft_DoesNotReturnMoreThanPool()
        {
            var upgrades = new List<UpgradeDefinition>
            {
                new()  { DisplayName = "A"},
                new()  { DisplayName = "B"}
            };

            var repository = new MockUpgradeRepository(upgrades);
            var draftSystem = new DraftSystem(repository);

            var draft = draftSystem.GenerateDraft(5);

            Assert.AreEqual(2, draft.Count);
        }
    }
}