using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests.Tests.EditModeTests
{
    public class SaveServiceTests
    {
        private static RunState CreateDummyRun()
        {
            return new RunState
            {
                version = 1,
                fightIndex = 5,
                player = new Unit("Tester")
                {
                    Stats = new Stats
                    {
                        MaxHP = 100,
                        CurrentHP = 42,
                        AttackPower = 7,
                        Armor = 3,
                        Speed = 10
                    }
                }
            };
        }

        [SetUp]
        public void CleanupBefore()
        {
            SaveService.Delete();
        }

        [TearDown]
        public void CleanupAfter()
        {
            SaveService.Delete();
        }

        [Test]
        public void Save_ThenLoad_RoundtripMatches()
        {
            var run = CreateDummyRun();

            SaveService.Save(run);
            var loaded = SaveService.Load();

            Assert.NotNull(loaded);
            Assert.AreEqual(5, loaded.fightIndex);
            Assert.AreEqual(42, loaded.player.Stats.CurrentHP);
            Assert.AreEqual(7, loaded.player.Stats.AttackPower);
        }

        [Test]
        public void Delete_RemovesSaveFile()
        {
            SaveService.Save(CreateDummyRun());

            Assert.IsTrue(SaveService.HasSave());

            SaveService.Delete();

            Assert.IsFalse(SaveService.HasSave());
        }

        [Test]
        public void HasSave_False_WhenNoFile()
        {
            Assert.IsFalse(SaveService.HasSave());
        }

        [Test]
        public void HasSave_True_AfterSave()
        {
            SaveService.Save(CreateDummyRun());

            Assert.IsTrue(SaveService.HasSave());
        }
    }
}