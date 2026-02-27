using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class DeathShieldTests
    {
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            _unit = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };
        }

        [Test]
        public void DeathShield_PreventsFirstDeath()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);

            _unit.ApplyDamage(null, 100);

            Assert.IsFalse(_unit.IsDead, "Unit should not be dead after DeathShield triggers");
        }

        [Test]
        public void DeathShield_RevivesWithHalfMaxHP()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);

            _unit.ApplyDamage(null, 100);

            Assert.AreEqual(50, _unit.Stats.CurrentHP, "Unit should revive with 50% max HP");
        }

        [Test]
        public void DeathShield_OnlyTriggersOnce()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);

            // First death - revived
            _unit.ApplyDamage(null, 100);
            Assert.IsFalse(_unit.IsDead, "Should be alive after first death");

            // Second death - should not revive
            _unit.ApplyDamage(null, 50);
            Assert.IsTrue(_unit.IsDead, "Should be dead after second death");
        }

        [Test]
        public void DeathShield_TriggeredFlag_SetAfterUse()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);

            Assert.IsFalse(shield.Triggered);

            _unit.ApplyDamage(null, 100);

            Assert.IsTrue(shield.Triggered);
        }

        [Test]
        public void DeathShield_CustomRevivePercent_Works()
        {
            var shield = new DeathShield(0.25f); // revive at 25%
            shield.OnAttach(_unit);

            _unit.ApplyDamage(null, 100);

            Assert.AreEqual(25, _unit.Stats.CurrentHP, "Should revive with 25% of max HP");
        }

        [Test]
        public void DeathShield_DoesNotTrigger_WhenUnitSurvives()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);

            _unit.ApplyDamage(null, 50);

            Assert.IsFalse(_unit.IsDead);
            Assert.IsFalse(shield.Triggered, "Shield should not trigger when unit survives");
            Assert.AreEqual(50, _unit.Stats.CurrentHP);
        }

        [Test]
        public void DeathShield_OnDetach_DoesNotRevive()
        {
            var shield = new DeathShield(0.5f);
            shield.OnAttach(_unit);
            shield.OnDetach(_unit);

            _unit.ApplyDamage(null, 100);

            Assert.IsTrue(_unit.IsDead, "Unit should stay dead after detach");
        }

        [Test]
        public void DeathShield_ViaArtifactApplier_WorksCorrectly()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Hourglass", "desc", Rarity.Epic, ArtifactTag.None,
                ArtifactEffectType.AddPassive, StatType.MaxHP, 0, "DeathShield", false);

            ArtifactApplier.ApplyToPlayer(artifact, _unit);

            Assert.AreEqual(1, _unit.Passives.Count);
            Assert.IsInstanceOf<DeathShield>(_unit.Passives[0]);

            _unit.ApplyDamage(null, 100);

            Assert.IsFalse(_unit.IsDead, "Applied artifact should prevent first death");
            Assert.AreEqual(50, _unit.Stats.CurrentHP);
        }
    }
}
