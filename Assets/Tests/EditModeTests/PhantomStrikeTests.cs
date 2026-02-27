using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class PhantomStrikeTests
    {
        private Unit _owner;
        private Unit _target;

        [SetUp]
        public void Setup()
        {
            _owner = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 20, Armor = 0, Speed = 5 } };
            _target = new Unit("Enemy") { Stats = new Stats { MaxHP = 200, CurrentHP = 200, AttackPower = 0, Armor = 0, Speed = 5 } };
        }

        [Test]
        public void PhantomStrike_DoesNotTriggerBefore5Hits()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            for (var i = 0; i < 4; i++)
                _owner.RaiseOnHit(_target, 10);

            Assert.AreEqual(200, _target.Stats.CurrentHP, "Target should be unharmed before 5th hit");
        }

        [Test]
        public void PhantomStrike_TriggersOnFifthHit()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 10);

            // 50% of 10 = 5 phantom damage
            Assert.AreEqual(195, _target.Stats.CurrentHP, "Target should take 5 phantom damage on 5th hit");
        }

        [Test]
        public void PhantomStrike_ResetsCounterAfterTrigger()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            // First trigger
            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 1);

            Assert.AreEqual(1, passive.HitCount, "Counter resets after trigger");
        }

        [Test]
        public void PhantomStrike_TriggersAgainAfterReset()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            // First trigger cycle
            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 10);

            var hpAfterFirst = _target.Stats.CurrentHP;

            // Second trigger cycle
            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 10);

            Assert.Less(_target.Stats.CurrentHP, hpAfterFirst, "Phantom strike should trigger a second time");
        }

        [Test]
        public void PhantomStrike_PhantomDamageIsHalfOfTriggeringHit()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            for (var i = 0; i < 4; i++)
                _owner.RaiseOnHit(_target, 20);

            _owner.RaiseOnHit(_target, 20); // 5th hit: 50% of 20 = 10 phantom damage

            Assert.AreEqual(190, _target.Stats.CurrentHP, "Phantom damage should be 50% of triggering hit");
        }

        [Test]
        public void PhantomStrike_DoesNotHitDeadTarget()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);

            _target.ApplyDamage(_owner, 200); // Kill target

            Assert.IsTrue(_target.IsDead);

            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 10);

            // No exception thrown, dead target not hit again
            Assert.AreEqual(0, _target.Stats.CurrentHP);
        }

        [Test]
        public void PhantomStrike_OnDetach_StopsCountingHits()
        {
            var passive = new PhantomStrike();
            passive.OnAttach(_owner);
            passive.OnDetach(_owner);

            for (var i = 0; i < 5; i++)
                _owner.RaiseOnHit(_target, 10);

            Assert.AreEqual(200, _target.Stats.CurrentHP, "Phantom should not trigger after detach");
        }

        [Test]
        public void PhantomStrike_ViaArtifactApplier_WorksCorrectly()
        {
            var artifact = UnityEngine.ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Crown of Echoes", "desc", Rarity.Epic, ArtifactTag.None,
                ArtifactEffectType.AddArtifact, StatType.MaxHP, 0, "PhantomStrike", false);

            ArtifactApplier.ApplyToPlayer(artifact, _owner);

            Assert.AreEqual(1, _owner.Passives.Count);
            Assert.IsInstanceOf<PhantomStrike>(_owner.Passives[0]);
        }
    }
}
