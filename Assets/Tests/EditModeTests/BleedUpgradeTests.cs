using System.Linq;

using Core;
using Core.Passives;
using Core.StatusEffects;

using NUnit.Framework;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="BleedUpgrade"/> passive.
    /// Verifies that the passive applies Bleed on hit, stacks accumulate, and detach stops applying.
    /// </summary>
    public class BleedUpgradeTests
    {
        private Unit _attacker;
        private Unit _target;

        [SetUp]
        public void Setup()
        {
            _attacker = new Unit("Hero")
            {
                Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 }
            };
            _target = new Unit("Enemy")
            {
                Stats = new Stats { MaxHP = 200, CurrentHP = 200, AttackPower = 0, Armor = 0, Speed = 5 }
            };
        }

        [Test]
        public void BleedUpgrade_AppliesBleedOnHit()
        {
            var passive = new BleedUpgrade(_attacker);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var hasBleed = _target.StatusEffects.Any(e => e.Id == "Bleed");
            Assert.IsTrue(hasBleed, "BleedUpgrade should apply Bleed status when owner hits a target");
        }

        [Test]
        public void BleedUpgrade_DefaultStacks_AreTwo()
        {
            var passive = new BleedUpgrade(_attacker);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var bleed = _target.StatusEffects.FirstOrDefault(e => e.Id == "Bleed");
            Assert.IsNotNull(bleed);
            Assert.AreEqual(2, bleed.Stacks, "Default BleedUpgrade should apply 2 stacks");
        }

        [Test]
        public void BleedUpgrade_CustomStacks_AreApplied()
        {
            var passive = new BleedUpgrade(_attacker, stacks: 4, duration: 3, baseDamage: 2);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var bleed = _target.StatusEffects.FirstOrDefault(e => e.Id == "Bleed");
            Assert.IsNotNull(bleed);
            Assert.AreEqual(4, bleed.Stacks, "Custom stacks should be applied");
        }

        [Test]
        public void BleedUpgrade_StacksAccumulate_OnMultipleHits()
        {
            var passive = new BleedUpgrade(_attacker, stacks: 2, duration: 3, baseDamage: 1);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);
            _attacker.RaiseOnHit(_target, 10);

            var bleed = _target.StatusEffects.FirstOrDefault(e => e.Id == "Bleed");
            Assert.IsNotNull(bleed, "Bleed should be applied");
            Assert.AreEqual(4, bleed.Stacks, "Bleed stacks should accumulate: 2 + 2 = 4");
        }

        [Test]
        public void BleedUpgrade_OnlyOneBleedEffect_OnTarget()
        {
            var passive = new BleedUpgrade(_attacker);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);
            _attacker.RaiseOnHit(_target, 10);

            var bleedCount = _target.StatusEffects.Count(e => e.Id == "Bleed");
            Assert.AreEqual(1, bleedCount, "Multiple hits should stack into one Bleed effect, not create duplicates");
        }

        [Test]
        public void BleedUpgrade_OnDetach_StopsApplyingBleed()
        {
            var passive = new BleedUpgrade(_attacker);
            passive.OnAttach(_attacker);
            passive.OnDetach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var hasBleed = _target.StatusEffects.Any(e => e.Id == "Bleed");
            Assert.IsFalse(hasBleed, "After OnDetach, BleedUpgrade should not apply Bleed on hit");
        }

        [Test]
        public void BleedUpgrade_DoesDamageOverTime()
        {
            var passive = new BleedUpgrade(_attacker, stacks: 5, duration: 3, baseDamage: 1);
            passive.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var hpBefore = _target.Stats.CurrentHP;
            _target.TickStatusesTurnStart();

            Assert.Less(_target.Stats.CurrentHP, hpBefore, "Bleed should deal damage on turn start");
        }

        [Test]
        public void BleedUpgrade_ViaApplyDamage_AppliesBleedToTarget()
        {
            var passive = new BleedUpgrade(_attacker);
            passive.OnAttach(_attacker);
            _attacker.Passives.Add(passive);

            // ApplyDamage fires OnHit only if target survives
            _target.ApplyDamage(_attacker, 10);

            var hasBleed = _target.StatusEffects.Any(e => e.Id == "Bleed");
            Assert.IsTrue(hasBleed, "Bleed should be applied when attacker deals damage and target survives");
        }
    }
}
