using NUnit.Framework;

namespace Tests.EditModeTests
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

        private static RunState CreateRunWithAbilitiesAndEffects()
        {
            var player = new Unit("Hero")
            {
                Stats = new Stats
                {
                    MaxHP = 100,
                    CurrentHP = 80,
                    AttackPower = 15,
                    Armor = 5,
                    Speed = 12
                }
            };

            // Add abilities
            player.Abilities.Add(new Fireball(player));

            // Add passives
            player.Passives.Add(new Thorns(player));
            player.Passives.Add(new Lifesteal(player, 0.2f));

            // Add status effects
            player.StatusEffects.Add(new Burn(3, 5));
            player.StatusEffects.Add(new Poison(2, 4, 1));
            player.StatusEffects.Add(new Bleed(3, 2, 1));

            return new RunState
            {
                version = 1,
                fightIndex = 10,
                player = player
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
        public void Save_WithAbilitiesPassivesAndEffects_LoadsCorrectly()
        {
            var run = CreateRunWithAbilitiesAndEffects();

            SaveService.Save(run);
            var loaded = SaveService.Load();

            Assert.NotNull(loaded);
            Assert.AreEqual(10, loaded.fightIndex);
            Assert.AreEqual("Hero", loaded.player.Name);
            Assert.AreEqual(80, loaded.player.Stats.CurrentHP);

            // Verify abilities are preserved
            Assert.AreEqual(1, loaded.player.Abilities.Count);
            Assert.IsInstanceOf<Fireball>(loaded.player.Abilities[0]);

            // Verify passives are preserved
            Assert.AreEqual(2, loaded.player.Passives.Count);
            Assert.IsInstanceOf<Thorns>(loaded.player.Passives[0]);
            Assert.IsInstanceOf<Lifesteal>(loaded.player.Passives[1]);

            // Verify status effects are preserved
            Assert.AreEqual(3, loaded.player.StatusEffects.Count);
            
            var burn = loaded.player.StatusEffects[0] as Burn;
            Assert.NotNull(burn);
            Assert.AreEqual("Burn", burn.Id);
            Assert.AreEqual(1, burn.Stacks);
            Assert.AreEqual(3, burn.Duration);

            var poison = loaded.player.StatusEffects[1] as Poison;
            Assert.NotNull(poison);
            Assert.AreEqual("Poison", poison.Id);
            Assert.AreEqual(2, poison.Stacks);
            Assert.AreEqual(4, poison.Duration);

            var bleed = loaded.player.StatusEffects[2] as Bleed;
            Assert.NotNull(bleed);
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(3, bleed.Stacks);
            Assert.AreEqual(2, bleed.Duration);
        }

        [Test]
        public void Save_PassiveWithParameters_WorksAfterLoad()
        {
            // Create a player with a Bleed passive that has specific stacks/duration
            var player = new Unit("Warrior")
            {
                Stats = new Stats
                {
                    MaxHP = 100,
                    CurrentHP = 100,
                    AttackPower = 20,
                    Armor = 5,
                    Speed = 10
                }
            };
            
            // Add a Bleed passive with custom parameters
            player.Passives.Add(new Bleed(player, stacks: 5, duration: 4));
            
            var run = new RunState
            {
                version = 1,
                fightIndex = 1,
                player = player
            };

            // Save and load
            SaveService.Save(run);
            var loaded = SaveService.Load();

            // Verify the passive was restored
            Assert.AreEqual(1, loaded.player.Passives.Count);
            var bleedPassive = loaded.player.Passives[0] as Bleed;
            Assert.NotNull(bleedPassive);

            // Verify the passive behavior works - when player hits, should apply bleed to target
            var enemy = new Unit("Enemy")
            {
                Stats = new Stats
                {
                    MaxHP = 50,
                    CurrentHP = 50,
                    AttackPower = 5,
                    Armor = 0,
                    Speed = 5
                }
            };

            // Trigger the passive by simulating a hit
            loaded.player.RaiseOnHit(enemy, 10);

            // Enemy should now have a bleed status effect with the saved parameters
            Assert.AreEqual(1, enemy.StatusEffects.Count);
            var appliedBleed = enemy.StatusEffects[0] as Bleed;
            Assert.NotNull(appliedBleed);
            Assert.AreEqual(5, appliedBleed.Stacks, "Bleed stacks should match the saved passive parameter");
            Assert.AreEqual(4, appliedBleed.Duration, "Bleed duration should match the saved passive parameter");
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