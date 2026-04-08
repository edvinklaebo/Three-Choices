using Core;
using Core.Passives;

using Interfaces;

using NUnit.Framework;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="Unit"/> lifecycle: Revive, RestorePlayerState, and ApplyDamage edge cases.
    /// </summary>
    public class UnitLifecycleTests
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

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            DamagePipeline.Clear();
        }

        // ---- Unit.Revive ----

        [Test]
        public void Revive_DeadUnit_IsAliveAfterRevive()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100);
            Assert.IsTrue(unit.IsDead);

            unit.Revive(50);

            Assert.IsFalse(unit.IsDead, "Revived unit should not be dead");
        }

        [Test]
        public void Revive_DeadUnit_HasCorrectHP()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100);

            unit.Revive(30);

            Assert.AreEqual(30, unit.Stats.CurrentHP, "Revived unit should have the specified HP");
        }

        [Test]
        public void Revive_ClampsHP_ToMaxHP()
        {
            var unit = CreateUnit("Hero", hp: 100);
            unit.ApplyDamage(null, 100);

            unit.Revive(999);

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Revive should not exceed MaxHP");
        }

        [Test]
        public void Revive_ClampsHP_ToMinimumOne()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100);

            unit.Revive(0);

            Assert.GreaterOrEqual(unit.Stats.CurrentHP, 1, "Revive with 0 HP should revive with at least 1 HP");
        }

        [Test]
        public void Revive_AliveUnit_IsNoOp()
        {
            var unit = CreateUnit("Hero");
            Assert.IsFalse(unit.IsDead);

            unit.Revive(50);

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Reviving an alive unit should not change its HP");
        }

        [Test]
        public void Revive_FiresHealthChangedEvent()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100);

            var healthChangedFired = false;
            unit.HealthChanged += (_, _, _) => healthChangedFired = true;

            unit.Revive(50);

            Assert.IsTrue(healthChangedFired, "Revive should fire HealthChanged event");
        }

        // ---- Unit.RestorePlayerState ----

        [Test]
        public void RestorePlayerState_ReattachesPassives()
        {
            var unit = CreateUnit("Hero");
            var rage = new Rage(unit);
            unit.Passives.Add(rage);

            // Simulate deserialization: passives are present but OnAttach was never called.
            // RestorePlayerState calls OnAttach on all passives.
            unit.RestorePlayerState();

            // After restore, Rage should be attached and apply damage bonus via DamagePipeline
            unit.Stats.CurrentHP = 50; // 50% HP

            var target = CreateUnit("Enemy");
            var ctx = new DamageContext(unit, target, 10);
            DamagePipeline.Process(ctx);

            // Rage: at 50% HP, missingHpPercent = 0.5, bonus = 1 + 0.5*1.0 = 1.5, finalValue = CeilToInt(10 * 1.5) = 15
            Assert.AreEqual(15, ctx.FinalValue,
                "After RestorePlayerState, Rage passive should apply damage bonus");
        }

        [Test]
        public void RestorePlayerState_SortsPassivesByPriority()
        {
            var unit = CreateUnit("Hero");
            var callOrder = new System.Collections.Generic.List<int>();

            var highPriorityPassive = new PriorityTrackingPassive(200, callOrder);
            var lowPriorityPassive = new PriorityTrackingPassive(10, callOrder);

            // Add in wrong order — restore should sort them
            unit.Passives.Add(highPriorityPassive);
            unit.Passives.Add(lowPriorityPassive);

            unit.RestorePlayerState();

            Assert.AreEqual(10, callOrder[0], "Lower-priority passive should be attached first");
            Assert.AreEqual(200, callOrder[1], "Higher-priority passive should be attached second");
        }

        // ---- Unit.ApplyDamage edge cases ----

        [Test]
        public void ApplyDamage_ZeroDamage_IsIgnored()
        {
            var unit = CreateUnit("Hero");
            var eventFired = false;
            unit.Damaged += (_, _, _) => eventFired = true;

            unit.ApplyDamage(null, 0);

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Zero damage should not change HP");
            Assert.IsFalse(eventFired, "Zero damage should not fire Damaged event");
        }

        [Test]
        public void ApplyDamage_NegativeDamage_IsIgnored()
        {
            var unit = CreateUnit("Hero");

            unit.ApplyDamage(null, -5);

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Negative damage should be ignored");
        }

        [Test]
        public void ApplyDamage_DeadUnit_IsIgnored()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100); // kill the unit
            Assert.IsTrue(unit.IsDead);

            var additionalDamageFired = false;
            unit.Damaged += (_, _, _) => additionalDamageFired = true;

            unit.ApplyDamage(null, 50);

            Assert.IsFalse(additionalDamageFired, "Dead unit should not receive additional damage");
        }

        [Test]
        public void ApplyDamage_KillingBlow_DoesNotFireOnHit()
        {
            var attacker = CreateUnit("Attacker");
            var target = CreateUnit("Target", hp: 10);
            var onHitFired = false;

            attacker.OnHit += (_, _, _) => onHitFired = true;

            // Deal lethal damage
            target.ApplyDamage(attacker, 10);

            Assert.IsTrue(target.IsDead, "Target should be dead after lethal damage");
            Assert.IsFalse(onHitFired,
                "OnHit should NOT fire when the hit kills the target (prevents status effects on corpse)");
        }

        [Test]
        public void ApplyDamage_NonLethalHit_DoesFireOnHit()
        {
            var attacker = CreateUnit("Attacker");
            var target = CreateUnit("Target", hp: 50);
            var onHitFired = false;

            attacker.OnHit += (_, _, _) => onHitFired = true;

            target.ApplyDamage(attacker, 10); // non-lethal

            Assert.IsFalse(target.IsDead);
            Assert.IsTrue(onHitFired, "OnHit should fire when target survives the hit");
        }

        [Test]
        public void ApplyDamage_KillingBlow_FiresDiedEvent()
        {
            var unit = CreateUnit("Hero");
            var diedFired = false;
            unit.Died += _ => diedFired = true;

            unit.ApplyDamage(null, 100);

            Assert.IsTrue(diedFired, "Killing blow should fire the Died event");
        }

        [Test]
        public void ApplyDamage_DyingEvent_CanCancelDeath()
        {
            var unit = CreateUnit("Hero", hp: 100);
            unit.Dying += (u, args) =>
            {
                args.Cancelled = true;
                args.ReviveHp = 10;
            };

            unit.ApplyDamage(null, 100);

            Assert.IsFalse(unit.IsDead, "Death should be cancelled by Dying subscriber");
            Assert.AreEqual(10, unit.Stats.CurrentHP, "Unit should have the ReviveHp after cancellation");
        }

        // ---- Unit.Heal edge cases ----

        [Test]
        public void Heal_DeadUnit_IsIgnored()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 100);
            Assert.IsTrue(unit.IsDead);

            unit.Heal(50);

            Assert.AreEqual(0, unit.Stats.CurrentHP, "Healing a dead unit should be ignored");
        }

        [Test]
        public void Heal_AtMaxHP_DoesNotExceedMaxHP()
        {
            var unit = CreateUnit("Hero", hp: 100);

            unit.Heal(50); // already at full HP

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Healing at max HP should not exceed MaxHP");
        }

        [Test]
        public void Heal_ZeroOrNegativeAmount_IsIgnored()
        {
            var unit = CreateUnit("Hero");
            unit.ApplyDamage(null, 50);
            var hpBefore = unit.Stats.CurrentHP;

            unit.Heal(0);
            unit.Heal(-10);

            Assert.AreEqual(hpBefore, unit.Stats.CurrentHP, "Healing for zero or negative should be ignored");
        }

        // ---- Helper passives ----

        private class PriorityTrackingPassive : IPassive
        {
            private readonly System.Collections.Generic.List<int> _callOrder;

            public int Priority { get; }

            public PriorityTrackingPassive(int priority, System.Collections.Generic.List<int> callOrder)
            {
                Priority = priority;
                _callOrder = callOrder;
            }

            public void OnAttach(Unit owner) => _callOrder.Add(Priority);
            public void OnDetach(Unit owner) { }
        }
    }
}
