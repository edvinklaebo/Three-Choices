using System;

using Core;
using Core.Abilities;
using Core.Artifacts;
using Core.Passives.Definitions;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class ObtainedItemTests
    {
        private Unit _player;

        [SetUp]
        public void SetUp()
        {
            _player = new Unit("Hero")
            {
                Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10 }
            };
        }

        [Test]
        public void NewPlayer_HasEmptyCompleteHistory()
        {
            Assert.IsEmpty(_player.ObtainedItems);
            Assert.IsTrue(_player.HasCompleteObtainedItemHistory);
        }

        [Test]
        public void RepeatedStatPicks_RecordCountAndStillApplyEveryTime()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();
            try
            {
                upgrade.EditorInit("attack", "Strength", StatType.AttackPower, 5);
                UpgradeApplier.Apply(upgrade, _player);
                UpgradeApplier.Apply(upgrade, _player);

                Assert.AreEqual(20, _player.Stats.AttackPower);
                Assert.AreEqual(1, _player.ObtainedItems.Count);
                Assert.AreEqual("attack", _player.ObtainedItems[0].Id);
                Assert.AreEqual("Strength", _player.ObtainedItems[0].DisplayName);
                Assert.AreEqual(ObtainedItemCategory.Upgrades, _player.ObtainedItems[0].Category);
                Assert.AreEqual(2, _player.ObtainedItems[0].Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(upgrade);
            }
        }

        [Test]
        public void AbilityAndPassivePicks_KeepSeparateCategoriesDespiteSharedIds()
        {
            var ability = ScriptableObject.CreateInstance<FireballDefinition>();
            var passive = ScriptableObject.CreateInstance<ThornsDefinition>();
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            try
            {
                ability.EditorInit("0", "Fireball", 10, 5);
                passive.EditorInit("0", "Thorns");
                stat.EditorInit("0", "Bulk Up", StatType.Armor, 2);

                UpgradeApplier.Apply(ability, _player);
                UpgradeApplier.Apply(ability, _player);
                UpgradeApplier.Apply(passive, _player);
                UpgradeApplier.Apply(stat, _player);

                Assert.AreEqual(3, _player.ObtainedItems.Count);
                Assert.AreEqual(ObtainedItemCategory.Abilities, _player.ObtainedItems[0].Category);
                Assert.AreEqual(2, _player.ObtainedItems[0].Count);
                Assert.AreEqual(ObtainedItemCategory.Passives, _player.ObtainedItems[1].Category);
                Assert.AreEqual(ObtainedItemCategory.Upgrades, _player.ObtainedItems[2].Category);
                Assert.AreEqual(1, _player.Abilities.Count);
                Assert.AreEqual(1, _player.Passives.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ability);
                UnityEngine.Object.DestroyImmediate(passive);
                UnityEngine.Object.DestroyImmediate(stat);
            }
        }

        [Test]
        public void FailedApplication_DoesNotRecordPick()
        {
            var upgrade = ScriptableObject.CreateInstance<StatDefinition>();
            try
            {
                upgrade.EditorInit("invalid", "Invalid", (StatType)999, 1);
                Assert.Throws<ArgumentOutOfRangeException>(() => UpgradeApplier.Apply(upgrade, _player));
                Assert.IsEmpty(_player.ObtainedItems);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(upgrade);
            }
        }

        [Test]
        public void ArtifactPicks_SnapshotDefinitionAndCountRepeatedAcquisitions()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            try
            {
                artifact.EditorInit(ArtifactId.HeartOfOak, "Heart of the Oak", "More health",
                    Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact);
                ArtifactApplier.ApplyToPlayer(artifact, _player);
                ArtifactApplier.ApplyToPlayer(artifact, _player);
                artifact.EditorInit(ArtifactId.HeartOfOak, "Changed asset", "Changed description",
                    Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact);

                Assert.AreEqual(1, _player.ObtainedItems.Count);
                Assert.AreEqual("Heart of the Oak", _player.ObtainedItems[0].DisplayName);
                Assert.AreEqual("More health", _player.ObtainedItems[0].Description);
                Assert.AreEqual(ObtainedItemCategory.Artifacts, _player.ObtainedItems[0].Category);
                Assert.AreEqual(2, _player.ObtainedItems[0].Count);
                Assert.AreEqual(2, _player.Artifacts.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(artifact);
            }
        }

        [Test]
        public void InvalidArtifact_DoesNotRecordPick()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            try
            {
                artifact.EditorInit((ArtifactId)999, "Invalid", string.Empty,
                    Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact);
                Assert.Throws<ArgumentOutOfRangeException>(() => ArtifactApplier.ApplyToPlayer(artifact, _player));
                Assert.IsEmpty(_player.ObtainedItems);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(artifact);
            }
        }

        [Test]
        public void SeparateRuns_DoNotShareHistory()
        {
            _player.RecordObtainedItem(new ObtainedItem("hp", "Fortitude", "", ObtainedItemCategory.Upgrades));
            Assert.IsEmpty(new Unit("Other hero").ObtainedItems);
        }

        [Test]
        public void LegacySave_CanRecordFuturePicksWithoutClaimingCompleteHistory()
        {
            var player = JsonUtility.FromJson<Unit>(
                "{\"Name\":\"Legacy\",\"Passives\":[],\"Abilities\":[],\"Artifacts\":[],\"StatusEffects\":[]}");
            player.RestorePlayerState();
            player.RecordObtainedItem(new ObtainedItem("hp", "Fortitude", "", ObtainedItemCategory.Upgrades));

            Assert.AreEqual(1, player.ObtainedItems.Count);
            Assert.IsFalse(player.HasCompleteObtainedItemHistory);
        }

        [Test]
        public void Restore_RemovesInvalidSerializedEntries()
        {
            JsonUtility.FromJsonOverwrite(
                "{\"_obtainedItems\":[{\"_id\":\"bad\",\"_displayName\":\"Bad\",\"_count\":0}]}", _player);

            _player.RestorePlayerState();

            Assert.IsEmpty(_player.ObtainedItems);
            Assert.IsFalse(_player.HasCompleteObtainedItemHistory);
        }

        [Test]
        public void InvalidItemIdentityOrCategory_FailsFast()
        {
            Assert.Throws<ArgumentException>(() =>
                new ObtainedItem("", "Name", "", ObtainedItemCategory.Upgrades));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ObtainedItem("id", "Name", "", (ObtainedItemCategory)999));
            Assert.Throws<ArgumentNullException>(() => _player.RecordObtainedItem(null));
        }
    }
}
