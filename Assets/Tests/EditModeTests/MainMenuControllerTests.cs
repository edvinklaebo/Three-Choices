using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class MainMenuControllerTests
    {
        [SetUp]
        public void Setup()
        {
            SaveService.Delete();
        }

        [TearDown]
        public void Cleanup()
        {
            SaveService.Delete();
        }

        [Test]
        public void ContinueButton_ShouldBeVisible_WhenSaveExists()
        {
            // Arrange
            var run = new RunState
            {
                fightIndex = 1,
                player = new Unit("Test")
                {
                    Stats = new Stats
                    {
                        MaxHP = 100,
                        CurrentHP = 100,
                        AttackPower = 7,
                        Armor = 3,
                        Speed = 10
                    }
                }
            };
            SaveService.Save(run);

            // Act & Assert
            Assert.IsTrue(SaveService.HasSave(), "Save should exist for Continue button to be visible");
        }

        [Test]
        public void ContinueButton_ShouldBeHidden_WhenNoSaveExists()
        {
            // Arrange
            SaveService.Delete();

            // Act & Assert
            Assert.IsFalse(SaveService.HasSave(), "No save should exist for Continue button to be hidden");
        }

        [Test]
        public void RunController_ContinueRun_LoadsSavedState()
        {
            // Arrange
            var originalRun = new RunState
            {
                fightIndex = 5,
                player = new Unit("Hero")
                {
                    Stats = new Stats
                    {
                        MaxHP = 120,
                        CurrentHP = 80,
                        AttackPower = 10,
                        Armor = 5,
                        Speed = 12
                    }
                }
            };
            SaveService.Save(originalRun);

            // Act
            var loaded = SaveService.Load();

            // Assert
            Assert.NotNull(loaded);
            Assert.AreEqual(5, loaded.fightIndex);
            Assert.AreEqual(80, loaded.player.Stats.CurrentHP);
            Assert.AreEqual(10, loaded.player.Stats.AttackPower);
        }
    }
}