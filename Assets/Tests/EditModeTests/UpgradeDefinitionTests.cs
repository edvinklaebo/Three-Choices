using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class UpgradeDefinitionTests
    {
        private UpgradeDefinition CreateUpgradeWithRarityWeight(int rarityWeight)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("test", "Test Upgrade");
            
            // Use reflection to set rarityWeight
            var field = typeof(UpgradeDefinition).GetField("rarityWeight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(upgrade, rarityWeight);
            
            return upgrade;
        }
        
        [Test]
        public void GetRarity_Returns_Common_For_Weight_100()
        {
            var upgrade = CreateUpgradeWithRarityWeight(100);
            Assert.AreEqual(Rarity.Common, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Uncommon_For_Weight_50()
        {
            var upgrade = CreateUpgradeWithRarityWeight(50);
            Assert.AreEqual(Rarity.Uncommon, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Rare_For_Weight_25()
        {
            var upgrade = CreateUpgradeWithRarityWeight(25);
            Assert.AreEqual(Rarity.Rare, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Epic_For_Weight_10()
        {
            var upgrade = CreateUpgradeWithRarityWeight(10);
            Assert.AreEqual(Rarity.Epic, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Common_For_Weight_Above_100()
        {
            var upgrade = CreateUpgradeWithRarityWeight(150);
            Assert.AreEqual(Rarity.Common, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Uncommon_For_Weight_Between_50_And_99()
        {
            var upgrade = CreateUpgradeWithRarityWeight(75);
            Assert.AreEqual(Rarity.Uncommon, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Rare_For_Weight_Between_25_And_49()
        {
            var upgrade = CreateUpgradeWithRarityWeight(30);
            Assert.AreEqual(Rarity.Rare, upgrade.GetRarity());
        }
        
        [Test]
        public void GetRarity_Returns_Epic_For_Weight_Below_25()
        {
            var upgrade = CreateUpgradeWithRarityWeight(15);
            Assert.AreEqual(Rarity.Epic, upgrade.GetRarity());
        }
    }
}
