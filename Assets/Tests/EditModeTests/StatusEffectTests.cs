using NUnit.Framework;
using System.Linq;

namespace Tests.EditModeTests
{
    public class StatusEffectTests
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
        public void PoisonEffect_DealsDamageAtTurnStart()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new PoisonEffect(5, 3);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(95, unit.Stats.CurrentHP, "Poison should deal 5 damage");
        }

        [Test]
        public void PoisonEffect_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new PoisonEffect(5, 3);

            unit.ApplyStatus(poison);
            
            Assert.AreEqual(3, poison.Duration, "Initial duration should be 3");
            
            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, poison.Duration, "Duration should decrease to 2");
            
            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, poison.Duration, "Duration should decrease to 1");
        }

        [Test]
        public void PoisonEffect_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new PoisonEffect(5, 2);

            unit.ApplyStatus(poison);
            
            Assert.AreEqual(1, unit.StatusEffects.Count, "Should have 1 status effect");
            
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();
            
            Assert.AreEqual(0, unit.StatusEffects.Count, "Poison should expire after duration reaches 0");
        }

        [Test]
        public void ApplyStatus_StacksExistingPoison()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison1 = new PoisonEffect(3, 2);
            var poison2 = new PoisonEffect(2, 2);

            unit.ApplyStatus(poison1);
            unit.ApplyStatus(poison2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have 1 poison effect");
            Assert.AreEqual(5, poison1.Stacks, "Stacks should add: 3 + 2 = 5");
        }

        [Test]
        public void PoisonEffect_CanKillUnit()
        {
            var unit = CreateUnit("Test", 10, 0, 0, 5);
            var poison = new PoisonEffect(15, 3);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.IsTrue(unit.isDead, "Poison should be able to kill the unit");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");
        }

        [Test]
        public void PoisonEffect_BypassesArmor()
        {
            var unit = CreateUnit("Tank", 100, 0, 1000, 5);
            var poison = new PoisonEffect(10, 3);

            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(90, unit.Stats.CurrentHP, "Poison should bypass armor and deal full 10 damage");
        }

        [Test]
        public void CombatSystem_TicksPoisonAtTurnStart()
        {
            var poisoned = CreateUnit("Poisoned", 50, 10, 0, 10);
            var enemy = CreateUnit("Enemy", 100, 5, 0, 5);

            poisoned.ApplyStatus(new PoisonEffect(5, 10));

            CombatSystem.RunFight(poisoned, enemy);

            // Poisoned unit should take poison damage each turn
            // They go first (speed 10 vs 5), so poison ticks before attacking
            // Should deal less damage overall because poison reduces their HP
            Assert.LessOrEqual(poisoned.Stats.CurrentHP, 0, "Poisoned unit should die");
        }

        [Test]
        public void PoisonEffect_KillsBeforeAttack()
        {
            var poisoned = CreateUnit("Poisoned", 5, 100, 0, 10);
            var enemy = CreateUnit("Enemy", 10, 0, 0, 5);

            poisoned.ApplyStatus(new PoisonEffect(10, 3));

            CombatSystem.RunFight(poisoned, enemy);

            Assert.IsTrue(poisoned.isDead, "Poisoned unit should die from poison");
            Assert.AreEqual(10, enemy.Stats.CurrentHP, "Enemy should take no damage because poisoned unit died before attacking");
        }

        [Test]
        public void ApplyDirectDamage_DoesNotTriggerDamagedEvent()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var damagedTriggered = false;

            unit.Damaged += (source, damage) => damagedTriggered = true;
            unit.ApplyDirectDamage(10);

            Assert.IsFalse(damagedTriggered, "Direct damage should not trigger Damaged event");
            Assert.AreEqual(90, unit.Stats.CurrentHP, "HP should still be reduced");
        }

        [Test]
        public void ApplyDirectDamage_TriggersHealthChangedEvent()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var healthChangedTriggered = false;
            var recordedHP = -1;

            unit.HealthChanged += (u, current, max) =>
            {
                healthChangedTriggered = true;
                recordedHP = current;
            };

            unit.ApplyDirectDamage(10);

            Assert.IsTrue(healthChangedTriggered, "Direct damage should trigger HealthChanged event");
            Assert.AreEqual(90, recordedHP, "Event should report correct HP");
        }

        [Test]
        public void MultipleStatusEffects_CanCoexist()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var poison = new PoisonEffect(5, 3);
            var mockEffect = new MockStatusEffect("Burn", 2, 2);

            unit.ApplyStatus(poison);
            unit.ApplyStatus(mockEffect);

            Assert.AreEqual(2, unit.StatusEffects.Count, "Should have 2 different status effects");
        }

        [Test]
        public void TickStatusesTurnEnd_CallsOnTurnEnd()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var mockEffect = new MockStatusEffect("Test", 1, 3);

            unit.ApplyStatus(mockEffect);
            unit.TickStatusesTurnEnd();

            Assert.IsTrue(mockEffect.TurnEndCalled, "OnTurnEnd should be called");
        }

        // Mock status effect for testing
        private class MockStatusEffect : IStatusEffect
        {
            public string Id { get; }
            public int Stacks { get; private set; }
            public int Duration { get; private set; }
            public bool TurnEndCalled { get; private set; }

            public MockStatusEffect(string id, int stacks, int duration)
            {
                Id = id;
                Stacks = stacks;
                Duration = duration;
            }

            public void OnApply(Unit target) { }
            public void OnTurnStart(Unit target) { Duration--; }
            public void OnTurnEnd(Unit target) { TurnEndCalled = true; }
            public void OnExpire(Unit target) { }
            public void AddStacks(int amount) { Stacks += amount; }
        }

        [Test]
        public void PoisonPassive_AppliesPoisonToAttackerOnHit()
        {
            var defender = CreateUnit("Defender", 100, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            // Apply poison passive to defender
            defender.Passives.Add(new Poison(defender, 2, 3));

            // Attacker hits defender
            defender.ApplyDamage(attacker, 10);

            // Verify attacker now has poison
            Assert.AreEqual(1, attacker.StatusEffects.Count, "Attacker should have poison status effect");
            var poison = attacker.StatusEffects[0];
            Assert.AreEqual("Poison", poison.Id);
            Assert.AreEqual(2, poison.Stacks, "Poison should have 2 stacks");
            Assert.AreEqual(3, poison.Duration, "Poison should have 3 turns duration");
        }

        [Test]
        public void PoisonPassive_StacksOnMultipleHits()
        {
            var defender = CreateUnit("Defender", 100, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);

            defender.Passives.Add(new Poison(defender, 2, 3));

            // First hit
            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(2, attacker.StatusEffects[0].Stacks);

            // Second hit
            defender.ApplyDamage(attacker, 10);
            Assert.AreEqual(4, attacker.StatusEffects[0].Stacks, "Poison stacks should accumulate to 4");
            Assert.AreEqual(1, attacker.StatusEffects.Count, "Should still only have one poison effect");
        }

        [Test]
        public void PoisonPassive_WorksInCombat()
        {
            var defender = CreateUnit("Defender", 20, 0, 0, 5);
            var attacker = CreateUnit("Attacker", 100, 5, 0, 10);

            // Defender has poison passive
            defender.Passives.Add(new Poison(defender, 3, 5));

            CombatSystem.RunFight(attacker, defender);

            // Attacker should die because:
            // 1. Attacker goes first (speed 10 vs 5), kills defender quickly
            // 2. But poison was applied on each hit and ticks on attacker's turns
            // 3. Defender has 0 attack so can't hurt attacker directly
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should be dead");
            Assert.Less(attacker.Stats.CurrentHP, 100, "Attacker should have taken poison damage since defender has 0 attack");
        }
    }
}
