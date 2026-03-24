using Core;
using Core.StatusEffects;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class RegenerationEffectTests
    {
        private static Unit CreateUnit(string name, int hp, int attack = 0, int armor = 0, int speed = 5)
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

        // ---- Healing ----

        [Test]
        public void Regeneration_HealsEachRound()
        {
            var unit = CreateUnit("Test", 50);
            unit.Stats.CurrentHP = 30;
            var regen = new Regeneration(stacks: 3, healingPerStack: 5);

            unit.ApplyStatus(regen);
            unit.TickStatusesTurnStart();

            // 3 stacks * 5 healing = 15 healed: 30 + 15 = 45
            Assert.AreEqual(45, unit.Stats.CurrentHP, "Should heal 3 stacks * 5 per stack = 15");
        }

        [Test]
        public void Regeneration_HealsBasedOnCurrentStacks()
        {
            var unit = CreateUnit("Test", 100);
            unit.Stats.CurrentHP = 10;
            var regen = new Regeneration(stacks: 2, healingPerStack: 3);

            unit.ApplyStatus(regen);

            // First tick: 2 stacks * 3 = 6 healed
            unit.TickStatusesTurnStart();
            Assert.AreEqual(16, unit.Stats.CurrentHP, "First tick should heal 2*3=6");

            // Second tick: 1 stack * 3 = 3 healed (one stack was removed)
            unit.TickStatusesTurnStart();
            Assert.AreEqual(19, unit.Stats.CurrentHP, "Second tick should heal 1*3=3");
        }

        [Test]
        public void Regeneration_DoesNotExceedMaxHP()
        {
            var unit = CreateUnit("Test", 100);
            unit.Stats.CurrentHP = 95;
            var regen = new Regeneration(stacks: 3, healingPerStack: 10);

            unit.ApplyStatus(regen);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(100, unit.Stats.CurrentHP, "HP should be capped at MaxHP");
        }

        [Test]
        public void Regeneration_DoesNotDealDamage()
        {
            var unit = CreateUnit("Test", 100);
            var regen = new Regeneration(stacks: 3, healingPerStack: 5);

            unit.ApplyStatus(regen);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Regeneration on full HP should not change HP");
        }

        // ---- Stack removal ----

        [Test]
        public void Regeneration_RemovesOneStackPerTick()
        {
            var unit = CreateUnit("Test", 100);
            var regen = new Regeneration(stacks: 3, healingPerStack: 1);

            unit.ApplyStatus(regen);

            Assert.AreEqual(3, regen.Stacks, "Initial stacks should be 3");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, regen.Stacks, "Should have 2 stacks after first tick");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, regen.Stacks, "Should have 1 stack after second tick");
        }

        // ---- Expiry ----

        [Test]
        public void Regeneration_ExpiresWhenStacksRunOut()
        {
            var unit = CreateUnit("Test", 100);
            unit.ApplyStatus(new Regeneration(stacks: 2, healingPerStack: 1));

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should have 1 status effect");

            unit.TickStatusesTurnStart(); // stacks: 2 -> 1
            unit.TickStatusesTurnStart(); // stacks: 1 -> 0 => expires

            Assert.AreEqual(0, unit.StatusEffects.Count, "Regeneration should expire when stacks reach 0");
        }

        [Test]
        public void Regeneration_DurationMatchesStacks()
        {
            var regen = new Regeneration(stacks: 4, healingPerStack: 2);

            Assert.AreEqual(4, regen.Duration, "Duration should equal initial stacks");

            var unit = CreateUnit("Test", 100);
            unit.ApplyStatus(regen);
            unit.TickStatusesTurnStart();

            Assert.AreEqual(3, regen.Duration, "Duration should match remaining stacks after one tick");
        }

        // ---- Stacking ----

        [Test]
        public void Regeneration_StacksAccumulateOnReapply()
        {
            var unit = CreateUnit("Test", 100);
            var regen1 = new Regeneration(stacks: 2, healingPerStack: 3);
            var regen2 = new Regeneration(stacks: 3, healingPerStack: 3);

            unit.ApplyStatus(regen1);
            unit.ApplyStatus(regen2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have one Regeneration effect");
            Assert.AreEqual(5, regen1.Stacks, "Stacks should add: 2 + 3 = 5");
        }

        [Test]
        public void Regeneration_KeepsHighestHealingPerStackOnReapply()
        {
            var unit = CreateUnit("Test", 100);
            unit.Stats.CurrentHP = 50;
            var regen1 = new Regeneration(stacks: 2, healingPerStack: 3);
            var regen2 = new Regeneration(stacks: 1, healingPerStack: 7);

            unit.ApplyStatus(regen1);
            unit.ApplyStatus(regen2);

            // 3 stacks with healingPerStack = max(3, 7) = 7
            unit.TickStatusesTurnStart();
            Assert.AreEqual(50 + 3 * 7, unit.Stats.CurrentHP, "Should use highest healingPerStack after merge");
        }

        // ---- Data-driven constructor ----

        private RegenerationDefinition _definition;

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(_definition);
                _definition = null;
            }
        }

        [Test]
        public void Regeneration_DataDrivenConstructor_ReadsAllValuesFromDefinition()
        {
            _definition = ScriptableObject.CreateInstance<RegenerationDefinition>();
            _definition.EditorInit(stacks: 4, healingPerStack: 6);

            var regen = new Regeneration(_definition);

            Assert.AreEqual(4, regen.Stacks);
            Assert.AreEqual(4, regen.Duration, "Duration should equal initial stacks");
            Assert.AreEqual(0, regen.BaseDamage, "BaseDamage should always be 0 for Regeneration");
            Assert.AreEqual("Regeneration", regen.Id);
        }

        // ---- Interface contract ----

        [Test]
        public void Regeneration_BaseDamageIsAlwaysZero()
        {
            var regen = new Regeneration(stacks: 3, healingPerStack: 5);
            Assert.AreEqual(0, regen.BaseDamage, "BaseDamage must be 0 — Regeneration does not deal damage");
        }

        [Test]
        public void Regeneration_IdIsRegenerationString()
        {
            var regen = new Regeneration(stacks: 1, healingPerStack: 1);
            Assert.AreEqual("Regeneration", regen.Id);
        }
    }
}
