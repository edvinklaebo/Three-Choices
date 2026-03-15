using System.Linq;

using Core;
using Core.Passives;
using Core.StatusEffects;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    public class PoisonBlocksHealingTests
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

        // ─── Direct Unit.Heal() ───────────────────────────────────────────────────

        [Test]
        public void Poison_BlocksDirectHeal()
        {
            var unit = CreateUnit("Hero", 100, 0, 0, 5);
            unit.Stats.CurrentHP = 50;
            unit.ApplyStatus(new Poison(3, 3, 1));

            unit.Heal(30);

            Assert.AreEqual(50, unit.Stats.CurrentHP, "Poison should block direct healing");
        }

        [Test]
        public void NoPoison_AllowsDirectHeal()
        {
            var unit = CreateUnit("Hero", 100, 0, 0, 5);
            unit.Stats.CurrentHP = 50;

            unit.Heal(30);

            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit without poison should be healed normally");
        }

        [Test]
        public void Poison_BlocksHeal_RegardlessOfHealAmount()
        {
            var unit = CreateUnit("Hero", 100, 0, 0, 5);
            unit.Stats.CurrentHP = 1;
            unit.ApplyStatus(new Poison(1, 5, 1));

            unit.Heal(99);

            Assert.AreEqual(1, unit.Stats.CurrentHP, "Poison should block even large heals");
        }

        [Test]
        public void Poison_Expired_NoLongerBlocksHeal()
        {
            var unit = CreateUnit("Hero", 100, 0, 0, 5);
            unit.Stats.CurrentHP = 50;
            unit.ApplyStatus(new Poison(1, 1, 1));

            // Tick once: damage fires and duration drops to 0 — poison removed
            unit.TickStatusesTurnStart();

            // Poison should now be gone
            Assert.AreEqual(0, unit.StatusEffects.Count, "Poison should have expired");

            // Healing should work again
            unit.Heal(20);
            Assert.AreEqual(69, unit.Stats.CurrentHP, "Healing should work after poison expires");
        }

        // ─── Healed event should NOT fire when poison blocks ─────────────────────

        [Test]
        public void Poison_BlocksHeal_HealedEventNotFired()
        {
            var unit = CreateUnit("Hero", 100, 0, 0, 5);
            unit.Stats.CurrentHP = 50;
            unit.ApplyStatus(new Poison(3, 3, 1));

            var healEventFired = false;
            unit.Healed += (_, _) => healEventFired = true;

            unit.Heal(30);

            Assert.IsFalse(healEventFired, "Healed event should not fire when poison blocks healing");
        }

        // ─── Lifesteal (in-combat healing) ───────────────────────────────────────

        [Test]
        public void Poison_BlocksLifestealHealing()
        {
            var attacker = CreateUnit("Vampire", 100, 20, 0, 10);
            attacker.Stats.CurrentHP = 50;
            var defender = CreateUnit("Victim", 100, 0, 0, 5);

            var lifesteal = new Lifesteal(attacker, 0.2f);
            lifesteal.OnAttach(attacker);
            attacker.Passives.Add(lifesteal);

            // Poison the attacker before combat
            attacker.ApplyStatus(new Poison(2, 10, 1));

            var hpBefore = attacker.Stats.CurrentHP;

            CombatSystem.RunFight(attacker, defender);

            Assert.LessOrEqual(attacker.Stats.CurrentHP, hpBefore,
                "Lifesteal should not heal an attacker who is poisoned");
        }

        [Test]
        public void Poison_BlocksLifesteal_NoHealActionsApplied()
        {
            var attacker = CreateUnit("Vampire", 100, 20, 0, 10);
            attacker.Stats.CurrentHP = 80;
            var defender = CreateUnit("Victim", 100, 0, 0, 5);

            var lifesteal = new Lifesteal(attacker, 0.2f);
            lifesteal.OnAttach(attacker);
            attacker.Passives.Add(lifesteal);

            attacker.ApplyStatus(new Poison(2, 10, 1));

            var actions = CombatSystem.RunFight(attacker, defender);

            // Any HealAction that was queued should have resolved to 0 actual HP gain
            var healActions = actions.OfType<HealAction>().ToList();
            foreach (var h in healActions)
                Assert.AreEqual(h.TargetHPBefore, h.TargetHPAfter,
                    "Heal action should not change HP when attacker is poisoned");
        }
    }
}
