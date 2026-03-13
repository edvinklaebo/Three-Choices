using Core;
using Core.Artifacts.Passives;
using Core.StatusEffects;

using NUnit.Framework;

using System.Linq;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for on-hit effect artifact passives: BlazingTorch, BloodRitual.
    /// These passives subscribe to Unit.OnHit and apply status effects.
    /// </summary>
    public class OnHitArtifactTests
    {
        private Unit _attacker;
        private Unit _target;

        [SetUp]
        public void Setup()
        {
            _attacker = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };
            _target = new Unit("Enemy") { Stats = new Stats { MaxHP = 200, CurrentHP = 200, AttackPower = 0, Armor = 0, Speed = 5 } };
        }

        // ---- BlazingTorch ----

        [Test]
        public void BlazingTorch_AppliesBurnOnHit()
        {
            var artifact = new BlazingTorch();
            artifact.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var hasBurn = _target.StatusEffects.Any(e => e.Id == "Burn");
            Assert.IsTrue(hasBurn, "BlazingTorch should apply Burn on hit");
        }

        [Test]
        public void BlazingTorch_DoesNotApplyToDeadTarget()
        {
            var artifact = new BlazingTorch();
            artifact.OnAttach(_attacker);

            _target.ApplyDamage(_attacker, 200); // Kill target
            Assert.IsTrue(_target.IsDead);

            _attacker.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count, "Should not apply Burn to dead target");
        }

        [Test]
        public void BlazingTorch_OnDetach_StopsApplyingBurn()
        {
            var artifact = new BlazingTorch();
            artifact.OnAttach(_attacker);
            artifact.OnDetach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count, "Should not apply Burn after detach");
        }

        [Test]
        public void BlazingTorch_BurnStacksOnMultipleHits()
        {
            var artifact = new BlazingTorch();
            artifact.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);
            _attacker.RaiseOnHit(_target, 10);

            // Burn doesn't stack (it refreshes), so should still be only 1 Burn effect
            var burnCount = _target.StatusEffects.Count(e => e.Id == "Burn");
            Assert.AreEqual(1, burnCount, "Burn should not create duplicate status entries");
        }

        // ---- BloodRitual ----

        [Test]
        public void BloodRitual_AppliesBleedOnHit()
        {
            var artifact = new BloodRitual();
            artifact.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            var hasBleed = _target.StatusEffects.Any(e => e.Id == "Bleed");
            Assert.IsTrue(hasBleed, "BloodRitual should apply Bleed on hit");
        }

        [Test]
        public void BloodRitual_BleedStacksOnMultipleHits()
        {
            var artifact = new BloodRitual(stacks: 2, bleedDuration: 3, bleedDamage: 2);
            artifact.OnAttach(_attacker);

            _attacker.RaiseOnHit(_target, 10);
            _attacker.RaiseOnHit(_target, 10);

            var bleed = _target.StatusEffects.FirstOrDefault(e => e.Id == "Bleed");
            Assert.IsNotNull(bleed, "Bleed should be applied");
            Assert.AreEqual(4, bleed.Stacks, "Bleed stacks should accumulate (2 + 2 = 4)");
        }

        [Test]
        public void BloodRitual_DoesNotApplyToDeadTarget()
        {
            var artifact = new BloodRitual();
            artifact.OnAttach(_attacker);

            _target.ApplyDamage(_attacker, 200); // Kill target
            Assert.IsTrue(_target.IsDead);

            _attacker.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count, "Should not apply Bleed to dead target");
        }

        [Test]
        public void BloodRitual_OnDetach_StopsApplyingBleed()
        {
            var artifact = new BloodRitual();
            artifact.OnAttach(_attacker);
            artifact.OnDetach(_attacker);

            _attacker.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count, "Should not apply Bleed after detach");
        }
    }
}
