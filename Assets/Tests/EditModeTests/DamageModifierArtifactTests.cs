using Core;
using Core.Artifacts.Passives;

using NUnit.Framework;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for IDamageModifier artifact passives: BerserkerMask, WarGauntlet, CorruptedTome.
    /// These passives are picked up by DamagePipeline from Unit.Artifacts.
    /// </summary>
    public class DamageModifierArtifactTests
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

        // ---- BerserkerMask ----

        [Test]
        public void BerserkerMask_AtFullHP_NoBonus()
        {
            var artifact = new BerserkerMask();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            _owner.Stats.CurrentHP = _owner.Stats.MaxHP; // Full HP

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            // At full HP: bonus = 1 + 0 = 1x multiplier
            Assert.AreEqual(10, ctx.FinalValue, "No damage bonus at full HP");
        }

        [Test]
        public void BerserkerMask_AtHalfHP_IncreaseDamage()
        {
            var artifact = new BerserkerMask();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            _owner.Stats.CurrentHP = 50; // 50% HP

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            // At 50% HP: bonus = 1 + 0.5 = 1.5x, 10 * 1.5 = 15
            Assert.AreEqual(15, ctx.FinalValue, "50% missing HP should give +50% damage");
        }

        [Test]
        public void BerserkerMask_DoesNotApplyToOtherAttacker()
        {
            var artifact = new BerserkerMask();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var otherAttacker = new Unit("Other") { Stats = new Stats { MaxHP = 100, CurrentHP = 50, AttackPower = 10, Armor = 0, Speed = 5 } };

            var ctx = new DamageContext(otherAttacker, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "BerserkerMask should not affect other attackers");
        }

        // ---- WarGauntlet ----

        [Test]
        public void WarGauntlet_AddsFlatDamage()
        {
            var artifact = new WarGauntlet();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(17, ctx.FinalValue, "WarGauntlet should add 7 flat damage");
        }

        [Test]
        public void WarGauntlet_DoesNotApplyToOtherAttacker()
        {
            var artifact = new WarGauntlet();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var otherAttacker = new Unit("Other") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };

            var ctx = new DamageContext(otherAttacker, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "WarGauntlet should not affect other attackers");
        }

        // ---- CorruptedTome ----

        [Test]
        public void CorruptedTome_MultipliesDamage()
        {
            var artifact = new CorruptedTome();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(13, ctx.FinalValue, "CorruptedTome should multiply damage by 1.3");
        }

        [Test]
        public void CorruptedTome_RoundsUpDamage()
        {
            var artifact = new CorruptedTome();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var ctx = new DamageContext(_owner, _target, 7);
            DamagePipeline.Process(ctx);

            // 7 * 1.3 = 9.1, rounds up to 10
            Assert.AreEqual(10, ctx.FinalValue, "CorruptedTome should round up damage");
        }

        [Test]
        public void CorruptedTome_DoesNotApplyToOtherAttacker()
        {
            var artifact = new CorruptedTome();
            artifact.OnAttach(_owner);
            _owner.Artifacts.Add(artifact);

            var otherAttacker = new Unit("Other") { Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 } };

            var ctx = new DamageContext(otherAttacker, _target, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "CorruptedTome should not affect other attackers");
        }

        // ---- Artifact vs Passive pipeline ----

        [Test]
        public void ArtifactModifiers_ApplyAlongsidePassiveModifiers()
        {
            // WarGauntlet in Artifacts, CritChance already tested in Passives
            var gauntlet = new WarGauntlet(5);
            gauntlet.OnAttach(_owner);
            _owner.Artifacts.Add(gauntlet);

            var tome = new CorruptedTome(2f);
            tome.OnAttach(_owner);
            _owner.Artifacts.Add(tome);

            var ctx = new DamageContext(_owner, _target, 10);
            DamagePipeline.Process(ctx);

            // WarGauntlet (priority 10) adds 5 first: 10 + 5 = 15
            // CorruptedTome (priority 100) multiplies: 15 * 2 = 30
            Assert.AreEqual(30, ctx.FinalValue, "Artifact modifiers should stack and respect priority");
        }
    }
}
