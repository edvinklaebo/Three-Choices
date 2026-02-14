using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class BleedEffectTests
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
        public void Bleed_DealsDamageAtTurnStart()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var bleed = new Bleed(5, 3);

            unit.ApplyStatus(bleed);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(95, unit.Stats.CurrentHP, "Bleed should deal 5 damage");
        }

        [Test]
        public void Bleed_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var bleed = new Bleed(5, 3);

            unit.ApplyStatus(bleed);

            Assert.AreEqual(3, bleed.Duration, "Initial duration should be 3");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, bleed.Duration, "Duration should decrease to 2");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, bleed.Duration, "Duration should decrease to 1");
        }

        [Test]
        public void Bleed_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var bleed = new Bleed(5, 2);

            unit.ApplyStatus(bleed);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should have 1 status effect");

            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(0, unit.StatusEffects.Count, "Bleed should expire after duration reaches 0");
        }

        [Test]
        public void ApplyStatus_StacksExistingBleed()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var bleed1 = new Bleed(3, 2);
            var bleed2 = new Bleed(2, 2);

            unit.ApplyStatus(bleed1);
            unit.ApplyStatus(bleed2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have 1 bleed effect");
            Assert.AreEqual(5, bleed1.Stacks, "Stacks should add: 3 + 2 = 5");
        }

        [Test]
        public void Bleed_CanKillUnit()
        {
            var unit = CreateUnit("Test", 10, 0, 0, 5);
            var bleed = new Bleed(15, 3);

            unit.ApplyStatus(bleed);
            unit.TickStatusesTurnStart();

            Assert.IsTrue(unit.isDead, "Bleed should be able to kill the unit");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");
        }

        [Test]
        public void Bleed_BypassesArmor()
        {
            var unit = CreateUnit("Tank", 100, 0, 1000, 5);
            var bleed = new Bleed(10, 3);

            unit.ApplyStatus(bleed);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(90, unit.Stats.CurrentHP, "Bleed should bypass armor and deal full 10 damage");
        }

        [Test]
        public void CombatSystem_TicksBleedAtTurnStart()
        {
            var bleeding = CreateUnit("Bleeding", 50, 10, 0, 10);
            var enemy = CreateUnit("Enemy", 100, 5, 0, 5);

            bleeding.ApplyStatus(new Bleed(5, 10));

            CombatSystem.RunFight(bleeding, enemy);

            // Bleeding unit should take bleed damage each turn
            // They go first (speed 10 vs 5), so bleed ticks before attacking
            // Should deal less damage overall because bleed reduces their HP
            Assert.LessOrEqual(bleeding.Stats.CurrentHP, 0, "Bleeding unit should die");
        }

        [Test]
        public void Bleed_KillsBeforeAttack()
        {
            var bleeding = CreateUnit("Bleeding", 5, 100, 0, 10);
            var enemy = CreateUnit("Enemy", 10, 0, 0, 5);

            bleeding.ApplyStatus(new Bleed(10, 3));

            CombatSystem.RunFight(bleeding, enemy);

            Assert.IsTrue(bleeding.isDead, "Bleeding unit should die from bleed");
            Assert.AreEqual(10, enemy.Stats.CurrentHP,
                "Enemy should take no damage because bleeding unit died before attacking");
        }

        [Test]
        public void BleedPassive_AppliesBleedToAttackerOnHit()
        {
            var defender = CreateUnit("Defender", 100, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            // Apply bleed passive to defender
            defender.Passives.Add(new Bleed(defender));

            // Attacker hits defender
            defender.ApplyDamage(attacker, 10);

            // Verify attacker now has bleed
            Assert.AreEqual(1, attacker.StatusEffects.Count, "Attacker should have bleed status effect");
            var bleed = attacker.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(2, bleed.Stacks, "Bleed should have 2 stacks");
            Assert.AreEqual(3, bleed.Duration, "Bleed should have 3 turns duration");
        }

        [Test]
        public void BleedPassive_StacksOnMultipleHits()
        {
            var defender = CreateUnit("Defender", 100, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            defender.Passives.Add(new Bleed(defender));

            // First hit
            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(2, attacker.StatusEffects[0].Stacks);

            // Second hit
            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(4, attacker.StatusEffects[0].Stacks, "Bleed stacks should accumulate to 4");
            Assert.AreEqual(1, attacker.StatusEffects.Count, "Should still only have one bleed effect");
        }

        [Test]
        public void BleedPassive_WorksInCombat()
        {
            var defender = CreateUnit("Defender", 20, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 5, 0, 10);

            // Defender has bleed passive
            defender.Passives.Add(new Bleed(defender, 3, 5));

            CombatSystem.RunFight(attacker, defender);

            // Attacker should die because:
            // 1. Attacker goes first (speed 10 vs 5), kills defender quickly
            // 2. But bleed was applied on each hit and ticks on attacker's turns
            // 3. Defender has 0 attack so can't hurt attacker directly
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should be dead");
            Assert.Less(attacker.Stats.CurrentHP, 100,
                "Attacker should have taken bleed damage since defender has 0 attack");
        }

        [Test]
        public void Bleed_And_Poison_CanCoexist()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var bleed = new Bleed(3, 3);
            var poison = new Poison(2, 2);

            unit.ApplyStatus(bleed);
            unit.ApplyStatus(poison);

            Assert.AreEqual(2, unit.StatusEffects.Count, "Should have both bleed and poison");
            
            unit.TickStatusesTurnStart();
            
            Assert.AreEqual(95, unit.Stats.CurrentHP, "Should take 5 damage total (3 from bleed + 2 from poison)");
        }
    }
}
