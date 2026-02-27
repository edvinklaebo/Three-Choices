using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class CritChancePassiveTests
    {
        private Unit _owner;
        private Unit _target;

        [SetUp]
        public void Setup()
        {
            DamagePipeline.Clear();
            _owner = new Unit("Hero") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };
            _target = new Unit("Enemy") { Stats = new Stats { MaxHP = 500, CurrentHP = 500, AttackPower = 0, Armor = 0, Speed = 5 } };
        }

        [TearDown]
        public void Teardown()
        {
            DamagePipeline.Clear();
        }

        [Test]
        public void CritChancePassive_AddedToPassives()
        {
            var passive = new CritChancePassive(0.1f);
            passive.OnAttach(_owner);
            _owner.Passives.Add(passive);

            Assert.AreEqual(1, _owner.Passives.Count);
            Assert.IsInstanceOf<CritChancePassive>(_owner.Passives[0]);
        }

        [Test]
        public void CritChancePassive_ZeroChance_NeverCrits()
        {
            var passive = new CritChancePassive(0f);
            passive.OnAttach(_owner);
            _owner.Passives.Add(passive);

            for (var i = 0; i < 50; i++)
            {
                var ctx = new DamageContext(_owner, _target, 10);
                DamagePipeline.Process(ctx);
                Assert.IsFalse(ctx.IsCritical, "Should never crit with 0% chance");
            }
        }

        [Test]
        public void CritChancePassive_FullChance_AlwaysCrits()
        {
            var passive = new CritChancePassive(1f);
            passive.OnAttach(_owner);
            _owner.Passives.Add(passive);

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.IsTrue(ctx.IsCritical, "Should always crit with 100% chance");
            Assert.AreEqual(20, ctx.FinalValue, "Crit should deal 2x damage");
        }

        [Test]
        public void CritChancePassive_DoesNotApplyToOtherUnit()
        {
            var passive = new CritChancePassive(1f);
            passive.OnAttach(_owner);
            _owner.Passives.Add(passive);

            var otherAttacker = new Unit("Other") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };

            var ctx = new DamageContext(otherAttacker, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.IsFalse(ctx.IsCritical, "Crit passive should not apply to other attackers");
        }

        [Test]
        public void CritChancePassive_DoesNotDoubleCrit_IfAlreadyCritical()
        {
            var passive = new CritChancePassive(1f);
            passive.OnAttach(_owner);
            _owner.Passives.Add(passive);

            var ctx = new DamageContext(_owner, _target, 10);
            ctx.IsCritical = true;
            ctx.FinalValue = 20; // Already critted by another modifier

            DamagePipeline.Process(ctx);

            Assert.AreEqual(20, ctx.FinalValue, "Should not apply crit multiplier twice");
        }

        [Test]
        public void CritChancePassive_ViaArtifactApplier_Works()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("test", "Lucky Horseshoe", "desc", Rarity.Uncommon, ArtifactTag.None,
                ArtifactEffectType.AddArtifact, StatType.MaxHP, 0, "CritChance", false);

            ArtifactApplier.ApplyToPlayer(artifact, _owner);

            Assert.AreEqual(1, _owner.Passives.Count);
            Assert.IsInstanceOf<CritChancePassive>(_owner.Passives[0]);
        }
    }
}
