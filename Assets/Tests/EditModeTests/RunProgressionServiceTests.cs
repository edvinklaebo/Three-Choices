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

        private static RunState CreateRun(int fightIndex = 0)
        {
            return new RunState
            {
                fightIndex = fightIndex,
                player = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100 } }
            };
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
    }
}
