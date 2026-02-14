using NUnit.Framework;

namespace Tests.EditModeTests.Tests.EditModeTests
{
    public class CombatSystemTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = armor,
                    Speed = speed
                }
            };
        }

        [Test]
        public void RunFight_CompletesWithWinner()
        {
            var unitA = CreateUnit("A", 10, 5, 0, 5);
            var unitB = CreateUnit("B", 10, 4, 0, 4);

            CombatSystem.RunFight(unitA, unitB);

            Assert.IsTrue(unitA.Stats.CurrentHP <= 0 || unitB.Stats.CurrentHP <= 0);
            Assert.IsTrue(unitA.Stats.CurrentHP > 0 || unitB.Stats.CurrentHP > 0);
        }

        [Test]
        public void RunFight_DoesNotModifyUnitsWithZeroHP()
        {
            var unitA = CreateUnit("A", 0, 5, 0, 5);
            var unitB = CreateUnit("B", 10, 4, 0, 4);

            CombatSystem.RunFight(unitA, unitB);

            Assert.AreEqual(0, unitA.Stats.CurrentHP);
            Assert.LessOrEqual(unitB.Stats.CurrentHP, 10);
        }

        [Test]
        public void FasterUnit_AttacksFirst_AndWins()
        {
            var fast = CreateUnit("Fast", 100, 10, 0, 20);
            var slow = CreateUnit("Slow", 100, 10, 0, 5);

            CombatSystem.RunFight(fast, slow);

            Assert.Greater(fast.Stats.CurrentHP, 0, "Fast unit should survive");
            Assert.LessOrEqual(slow.Stats.CurrentHP, 0, "Slow unit should die");
        }

        [Test]
        public void EqualSpeed_AttackerStarts_AndWins()
        {
            var attacker = CreateUnit("Attacker", 30, 10, 0, 10);
            var defender = CreateUnit("Defender", 30, 10, 0, 10);

            CombatSystem.RunFight(attacker, defender);

            // Because >= is used, attacker always starts on tie and should win
            Assert.Greater(attacker.Stats.CurrentHP, 0);
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0);
        }

        [Test]
        public void ArmorReducesDamage_Correctly()
        {
            var attacker = CreateUnit("Attacker", 100, 50, 0, 10);
            var defender = CreateUnit("Tank", 100, 0, 100, 1);

            CombatSystem.RunFight(attacker, defender);

            // Multiplier = 100 / (100 + 100) = 0.5
            // Damage per hit = ceil(50 * 0.5) = 25
            // Defender HP: 100 -> 75 -> 50 -> 25 -> 0
            Assert.AreEqual(0, defender.Stats.CurrentHP);
            Assert.Greater(attacker.Stats.CurrentHP, 0);
        }

        [Test]
        public void HighArmorStillTakesAtLeastOneDamage()
        {
            var attacker = CreateUnit("Needle", 100, 1, 0, 10);
            var defender = CreateUnit("Wall", 5, 0, 10000, 1);

            CombatSystem.RunFight(attacker, defender);

            // Formula never reaches zero, so defender must eventually die
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0);
        }

        [Test]
        public void BothDealDamage_ExactlyOneSurvives()
        {
            var a = CreateUnit("A", 40, 10, 0, 10);
            var b = CreateUnit("B", 40, 10, 0, 5);

            CombatSystem.RunFight(a, b);

            var aAlive = a.Stats.CurrentHP > 0;
            var bAlive = b.Stats.CurrentHP > 0;

            Assert.IsTrue(aAlive ^ bAlive, "Exactly one unit should be alive at the end");
        }
    }
}