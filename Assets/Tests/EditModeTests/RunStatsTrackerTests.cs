using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class RunStatsTrackerTests
    {
        private static Unit CreateUnit(string name, int hp = 100, int attack = 10, int armor = 0, int speed = 5)
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
        public void DamageDealt_AccumulatesFromPlayerOnHit()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            tracker.RegisterPlayer(player);
            enemy.ApplyDamage(player, 20);

            Assert.AreEqual(20, tracker.Stats.TotalDamageDealt);
        }

        [Test]
        public void DamageDealt_AccumulatesAcrossMultipleHits()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy", hp: 200);

            tracker.RegisterPlayer(player);
            enemy.ApplyDamage(player, 10);
            enemy.ApplyDamage(player, 15);

            Assert.AreEqual(25, tracker.Stats.TotalDamageDealt);
        }

        [Test]
        public void DamageTaken_AccumulatesFromPlayerDamaged()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var attacker = CreateUnit("Enemy");

            tracker.RegisterPlayer(player);
            player.ApplyDamage(attacker, 15);

            Assert.AreEqual(15, tracker.Stats.TotalDamageTaken);
        }

        [Test]
        public void HealingDone_AccumulatesFromPlayerHealed()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player", hp: 100);

            tracker.RegisterPlayer(player);
            player.ApplyDamage(null, 50);
            player.Heal(30);

            Assert.AreEqual(30, tracker.Stats.TotalHealingDone);
        }

        [Test]
        public void HealingDone_CapsAtMaxHP()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player", hp: 100);

            tracker.RegisterPlayer(player);
            player.ApplyDamage(null, 20);
            player.Heal(50); // only 20 can actually be healed

            Assert.AreEqual(20, tracker.Stats.TotalHealingDone);
        }

        [Test]
        public void EnemiesKilled_IncrementsOnEnemyDeath()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy", hp: 10);

            tracker.RegisterEnemy(enemy);
            enemy.ApplyDamage(player, 10);

            Assert.AreEqual(1, tracker.Stats.EnemiesKilled);
        }

        [Test]
        public void FightsCompleted_IncreasesOnIncrement()
        {
            var tracker = new RunStatsTracker();
            tracker.IncrementFightsCompleted();
            tracker.IncrementFightsCompleted();

            Assert.AreEqual(2, tracker.Stats.FightsCompleted);
        }

        [Test]
        public void Reset_ClearsAllStats()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy", hp: 10);

            tracker.RegisterPlayer(player);
            tracker.RegisterEnemy(enemy);
            enemy.ApplyDamage(player, 10);
            tracker.IncrementFightsCompleted();

            tracker.Reset();

            Assert.AreEqual(0, tracker.Stats.TotalDamageDealt);
            Assert.AreEqual(0, tracker.Stats.TotalDamageTaken);
            Assert.AreEqual(0, tracker.Stats.TotalHealingDone);
            Assert.AreEqual(0, tracker.Stats.EnemiesKilled);
            Assert.AreEqual(0, tracker.Stats.FightsCompleted);
        }

        [Test]
        public void UnregisterPlayer_StopsAccumulatingDamageDealt()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            tracker.RegisterPlayer(player);
            tracker.UnregisterPlayer(player);
            enemy.ApplyDamage(player, 20);

            Assert.AreEqual(0, tracker.Stats.TotalDamageDealt);
        }

        [Test]
        public void UnregisterEnemy_StopsTrackingKills()
        {
            var tracker = new RunStatsTracker();
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy", hp: 10);

            tracker.RegisterEnemy(enemy);
            tracker.UnregisterEnemy(enemy);
            enemy.ApplyDamage(player, 10);

            Assert.AreEqual(0, tracker.Stats.EnemiesKilled);
        }

        [Test]
        public void RunStats_ToViewData_ReturnsAllFiveStats()
        {
            var stats = new RunStats
            {
                TotalDamageDealt = 100,
                TotalDamageTaken = 50,
                TotalHealingDone = 30,
                EnemiesKilled = 5,
                FightsCompleted = 3
            };

            var viewData = new System.Collections.Generic.List<StatViewData>(stats.ToViewData());

            Assert.AreEqual(5, viewData.Count);
            Assert.AreEqual("Damage Dealt", viewData[0].Name);
            Assert.AreEqual(100, viewData[0].Value);
            Assert.AreEqual("Enemies Killed", viewData[3].Name);
            Assert.AreEqual(5, viewData[3].Value);
        }
    }
}
