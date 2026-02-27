using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class PoisonAmplifierTests
    {
        private Unit _owner;
        private Unit _target;

        [SetUp]
        public void Setup()
        {
            _owner = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };
            _target = new Unit("Enemy") { Stats = new Stats { MaxHP = 200, CurrentHP = 200, AttackPower = 0, Armor = 0, Speed = 5 } };
        }

        private void AttachBothPassives()
        {
            var poisonUpgrade = new PoisonUpgrade(_owner, stacks: 2, duration: 3, baseDamage: 2);
            poisonUpgrade.OnAttach(_owner);
            _owner.Passives.Add(poisonUpgrade);

            var amplifier = new PoisonAmplifier(bonusStacks: 2, bonusDuration: 3, bonusBaseDamage: 2);
            amplifier.OnAttach(_owner);
            _owner.Passives.Add(amplifier);
        }

        [Test]
        public void PoisonAmplifier_Alone_DoesNotApplyPoison_WithoutExistingPoison()
        {
            var amplifier = new PoisonAmplifier();
            amplifier.OnAttach(_owner);

            _owner.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count,
                "Amplifier should not apply poison if no Poison was applied first");
        }

        [Test]
        public void PoisonAmplifier_WithPoisonUpgrade_DoublesStacks()
        {
            AttachBothPassives();

            _owner.RaiseOnHit(_target, 10);

            Assert.AreEqual(1, _target.StatusEffects.Count);
            var poison = _target.StatusEffects[0] as Poison;
            Assert.NotNull(poison);
            Assert.AreEqual(4, poison.Stacks, "Stacks should be doubled (2 from PoisonUpgrade + 2 from Amplifier)");
        }

        [Test]
        public void PoisonAmplifier_DoesNotApplyExtraPoison_IfTargetHasNoPoison()
        {
            var amplifier = new PoisonAmplifier(2, 3, 2);
            amplifier.OnAttach(_owner);

            _owner.RaiseOnHit(_target, 10);

            Assert.AreEqual(0, _target.StatusEffects.Count,
                "Amplifier should only fire when target has Poison");
        }

        [Test]
        public void PoisonAmplifier_OnDetach_StopsDoubling()
        {
            var poisonUpgrade = new PoisonUpgrade(_owner);
            poisonUpgrade.OnAttach(_owner);

            var amplifier = new PoisonAmplifier();
            amplifier.OnAttach(_owner);
            amplifier.OnDetach(_owner);

            _owner.RaiseOnHit(_target, 10);

            var poison = _target.StatusEffects[0] as Poison;
            Assert.AreEqual(2, poison.Stacks, "Stacks should not be doubled after detach");
        }

        [Test]
        public void PoisonAmplifier_ViaArtifactApplier_Works()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Poison Tipped Darts", "desc", Rarity.Uncommon, ArtifactTag.Poison,
                ArtifactEffectType.AddPassive, StatType.MaxHP, 0, "PoisonAmplifier", false);

            ArtifactApplier.ApplyToPlayer(artifact, _owner);

            Assert.AreEqual(1, _owner.Passives.Count);
            Assert.IsInstanceOf<PoisonAmplifier>(_owner.Passives[0]);
        }

        [Test]
        public void PoisonAmplifier_DoesNotAffectDeadTarget()
        {
            AttachBothPassives();

            _target.ApplyDamage(_owner, 200);
            Assert.IsTrue(_target.IsDead);

            // Should not throw
            _owner.RaiseOnHit(_target, 10);
        }
    }
}
