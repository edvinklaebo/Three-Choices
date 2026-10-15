using System;
using Core;
using NUnit.Framework;
using UI;

namespace Tests.EditModeTests
{
    public class ObtainedItemsFormatterTests
    {
        [Test]
        public void Format_GroupsSnapshotsAndShowsNamesDescriptionsAndCounts()
        {
            var player = new Unit("Player");
            var upgrade = new ObtainedItem("damage", "Sharper blade", "+2 damage", ObtainedItemCategory.Upgrades);
            player.RecordObtainedItem(upgrade);
            player.RecordObtainedItem(new ObtainedItem("damage", "Sharper blade", "+2 damage",
                ObtainedItemCategory.Upgrades));
            player.RecordObtainedItem(new ObtainedItem("fire", "Fireball", "Burn the enemy",
                ObtainedItemCategory.Abilities));
            player.RecordObtainedItem(new ObtainedItem("regen", "Regeneration", "Recover health",
                ObtainedItemCategory.Passives));
            player.RecordObtainedItem(new ObtainedItem("ring", "Ring", "An ancient ring",
                ObtainedItemCategory.Artifacts));

            var text = ObtainedItemsFormatter.Format(player.ObtainedItems);

            StringAssert.Contains("Sharper blade x2", text);
            StringAssert.Contains("+2 damage", text);
            StringAssert.Contains("Fireball x1", text);
            StringAssert.Contains("Burn the enemy", text);
            StringAssert.Contains("Regeneration x1", text);
            StringAssert.Contains("Recover health", text);
            StringAssert.Contains("Ring x1", text);
            StringAssert.Contains("An ancient ring", text);
            Assert.Less(text.IndexOf("Upgrades", StringComparison.Ordinal),
                text.IndexOf("Abilities", StringComparison.Ordinal));
            Assert.Less(text.IndexOf("Abilities", StringComparison.Ordinal),
                text.IndexOf("Passives", StringComparison.Ordinal));
            Assert.Less(text.IndexOf("Passives", StringComparison.Ordinal),
                text.IndexOf("Artifacts", StringComparison.Ordinal));
            Assert.AreEqual(4, player.ObtainedItems.Count);
            Assert.AreEqual(1, upgrade.Count);
            Assert.AreEqual(2, player.ObtainedItems[0].Count);
        }

        [Test]
        public void Format_EmptyHistoryShowsEveryCategoryWithEmptyState()
        {
            var text = ObtainedItemsFormatter.Format(Array.Empty<ObtainedItem>());

            foreach (ObtainedItemCategory category in Enum.GetValues(typeof(ObtainedItemCategory)))
                StringAssert.Contains(category + "\nNo recorded items.", text.Replace("\r\n", "\n"));
            StringAssert.DoesNotContain("older save", text);
        }

        [Test]
        public void Format_IncompleteHistoryExplainsUnavailableOlderAcquisitions()
        {
            var text = ObtainedItemsFormatter.Format(Array.Empty<ObtainedItem>(), false);

            StringAssert.Contains("older save", text);
            StringAssert.Contains("unavailable", text);
            StringAssert.Contains("Only newly recorded items", text);
        }

        [Test]
        public void Format_MissingPlayerDoesNotClaimInventoryIsEmpty()
        {
            StringAssert.Contains("not yet available", ObtainedItemsFormatter.Format(null));
        }
    }
}
