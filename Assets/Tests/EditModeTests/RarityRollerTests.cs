using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class RarityRollerTests
    {
        [Test]
        public void RollRarity_ReturnsValidRarity()
        {
            var roller = new RarityRoller();

            var rarity = roller.RollRarity();

            Assert.IsTrue(rarity == Rarity.Common ||
                          rarity == Rarity.Uncommon ||
                          rarity == Rarity.Rare ||
                          rarity == Rarity.Epic);
        }

        [Test]
        public void RollRarity_DistributionFavorsCommon()
        {
            var roller = new RarityRoller();
            var counts = new Dictionary<Rarity, int>
            {
                { Rarity.Common, 0 },
                { Rarity.Uncommon, 0 },
                { Rarity.Rare, 0 },
                { Rarity.Epic, 0 }
            };

            // Roll 1000 times to test distribution
            for (var i = 0; i < 1000; i++)
            {
                var rarity = roller.RollRarity();
                counts[rarity]++;
            }

            // Common should be most frequent
            Assert.Greater(counts[Rarity.Common], counts[Rarity.Uncommon]);
            Assert.Greater(counts[Rarity.Uncommon], counts[Rarity.Rare]);
            Assert.Greater(counts[Rarity.Rare], counts[Rarity.Epic]);
        }
    }
}