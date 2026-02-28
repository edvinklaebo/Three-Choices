using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class RunProgressionServiceTests
    {
        [TearDown]
        public void Cleanup()
        {
            SaveService.Delete();
        }

        private FightStartedEventChannel CreateChannel()
        {
            return ScriptableObject.CreateInstance<FightStartedEventChannel>();
        }

        private BossFightEventChannel CreateBossChannel()
        {
            return ScriptableObject.CreateInstance<BossFightEventChannel>();
        }

        private static RunState CreateRun(int fightIndex = 0)
        {
            return new RunState
            {
                fightIndex = fightIndex,
                player = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } }
            };
        }

        private static BossManager CreateBossManager(params BossDefinition[] bosses)
        {
            var registry = ScriptableObject.CreateInstance<BossRegistry>();
            foreach (var boss in bosses)
                registry.EditorAddBoss(boss);
            return new BossManager(registry);
        }

        private static BossDefinition CreateBoss(string id, int difficultyRating = 1)
        {
            var boss = ScriptableObject.CreateInstance<BossDefinition>();
            boss.EditorInit(id, id, difficultyRating);
            return boss;
        }

        [Test]
        public void HandleNextFight_RaisesFightStartedWithCorrectIndex()
        {
            var channel = CreateChannel();
            var service = new RunProgressionService(channel);
            var run = CreateRun(fightIndex: 2);
            service.SetRun(run, run.player);

            Unit receivedPlayer = null;
            var receivedIndex = -1;
            channel.OnRaised += (p, i) => { receivedPlayer = p; receivedIndex = i; };

            service.HandleNextFight();

            Assert.AreEqual(run.player, receivedPlayer);
            Assert.AreEqual(2, receivedIndex);

            Object.DestroyImmediate(channel);
        }

        [Test]
        public void HandleNextFight_IncrementsFightIndex()
        {
            var channel = CreateChannel();
            var service = new RunProgressionService(channel);
            var run = CreateRun(fightIndex: 0);
            service.SetRun(run, run.player);

            service.HandleNextFight();
            service.HandleNextFight();

            Assert.AreEqual(2, run.fightIndex);

            Object.DestroyImmediate(channel);
        }

        [Test]
        public void HandleNextFight_UpdatesRunStateFightIndex()
        {
            var channel = CreateChannel();
            var service = new RunProgressionService(channel);
            var run = CreateRun(fightIndex: 5);
            service.SetRun(run, run.player);

            service.HandleNextFight();

            Assert.AreEqual(6, run.fightIndex);

            Object.DestroyImmediate(channel);
        }

        [Test]
        public void SetRun_RestoresFightIndexFromRunState()
        {
            var channel = CreateChannel();
            var service = new RunProgressionService(channel);
            var run = CreateRun(fightIndex: 7);
            service.SetRun(run, run.player);

            var receivedIndex = -1;
            channel.OnRaised += (_, i) => receivedIndex = i;
            service.HandleNextFight();

            Assert.AreEqual(7, receivedIndex);

            Object.DestroyImmediate(channel);
        }

        // ── Boss fight integration ─────────────────────────────────────────────

        [Test]
        public void HandleNextFight_RaisesBossEvent_WhenFightIndexIsBossFight()
        {
            var channel = CreateChannel();
            var bossChannel = CreateBossChannel();
            var boss = CreateBoss("big_boss");
            var service = new RunProgressionService(channel, CreateBossManager(boss), bossChannel);
            var run = CreateRun(fightIndex: 10);
            service.SetRun(run, run.player);

            BossDefinition receivedBoss = null;
            bossChannel.OnRaised += b => receivedBoss = b;

            service.HandleNextFight();

            Assert.IsNotNull(receivedBoss, "Boss fight event should fire at fight index 10");

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(bossChannel);
        }

        [Test]
        public void HandleNextFight_DoesNotRaiseBossEvent_WhenFightIndexIsNotBossFight()
        {
            var channel = CreateChannel();
            var bossChannel = CreateBossChannel();
            var boss = CreateBoss("big_boss");
            var service = new RunProgressionService(channel, CreateBossManager(boss), bossChannel);
            var run = CreateRun(fightIndex: 5);
            service.SetRun(run, run.player);

            var bossEventFired = false;
            bossChannel.OnRaised += _ => bossEventFired = true;

            service.HandleNextFight();

            Assert.IsFalse(bossEventFired, "Boss fight event should NOT fire for non-boss fight");

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(bossChannel);
        }

        [Test]
        public void HandleNextFight_DoesNotRaiseBossEvent_WhenNoBossManagerProvided()
        {
            var channel = CreateChannel();
            var bossChannel = CreateBossChannel();
            var service = new RunProgressionService(channel, bossManager: null, bossFightStarted: bossChannel);
            var run = CreateRun(fightIndex: 10);
            service.SetRun(run, run.player);

            var bossEventFired = false;
            bossChannel.OnRaised += _ => bossEventFired = true;

            service.HandleNextFight();

            Assert.IsFalse(bossEventFired, "No boss event should fire without a BossManager");

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(bossChannel);
        }

        [Test]
        public void HandleNextFight_RaisesNormalFightEvent_EvenOnBossFight()
        {
            var channel = CreateChannel();
            var bossChannel = CreateBossChannel();
            var boss = CreateBoss("big_boss");
            var service = new RunProgressionService(channel, CreateBossManager(boss), bossChannel);
            var run = CreateRun(fightIndex: 10);
            service.SetRun(run, run.player);

            var normalFightFired = false;
            channel.OnRaised += (_, _) => normalFightFired = true;

            service.HandleNextFight();

            Assert.IsTrue(normalFightFired, "FightStarted event must always fire, even on boss fights");

            Object.DestroyImmediate(channel);
            Object.DestroyImmediate(bossChannel);
        }
    }
}

