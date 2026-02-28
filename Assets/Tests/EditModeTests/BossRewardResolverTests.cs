using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class BossRewardResolverTests
    {
        private BossDefinition CreateBoss(string id, ArtifactDefinition reward = null)
        {
            var boss = ScriptableObject.CreateInstance<BossDefinition>();
            boss.EditorInit(id, id, 1, reward);
            return boss;
        }

        private ArtifactDefinition CreateArtifact(string id)
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(id, id, "desc", Rarity.Common, ArtifactTag.None,
                ArtifactEffectType.AddArtifact);
            return artifact;
        }

        [Test]
        public void ResolveReward_ReturnsArtifact_AssignedToBoss()
        {
            var artifact = CreateArtifact("artifact_fire_rune");
            var boss = CreateBoss("fire_boss", artifact);
            var resolver = new BossRewardResolver();

            var result = resolver.ResolveReward(boss);

            Assert.AreEqual(artifact, result);
        }

        [Test]
        public void ResolveReward_ReturnsNull_WhenBossHasNoArtifact()
        {
            var boss = CreateBoss("empty_boss");
            var resolver = new BossRewardResolver();

            var result = resolver.ResolveReward(boss);

            Assert.IsNull(result);
        }

        [Test]
        public void ResolveReward_ReturnsNull_WhenBossIsNull()
        {
            var resolver = new BossRewardResolver();

            var result = resolver.ResolveReward(null);

            Assert.IsNull(result);
        }

        [Test]
        public void ResolveReward_ReturnsDifferentArtifacts_ForDifferentBosses()
        {
            var artifact1 = CreateArtifact("artifact_1");
            var artifact2 = CreateArtifact("artifact_2");
            var boss1 = CreateBoss("boss_1", artifact1);
            var boss2 = CreateBoss("boss_2", artifact2);
            var resolver = new BossRewardResolver();

            Assert.AreEqual(artifact1, resolver.ResolveReward(boss1));
            Assert.AreEqual(artifact2, resolver.ResolveReward(boss2));
        }
    }
}
