using Core;
using Core.Artifacts.Passives;

using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class StatBoostArtifactTests
    {
        private Unit _unit;

        [SetUp]
        public void Setup()
        {
            _unit = new Unit("Hero")
            {
                Stats = new Stats
                {
                    MaxHP = 100,
                    CurrentHP = 80,
                    AttackPower = 10,
                    Armor = 5,
                    Speed = 3
                }
            };
        }

        // ---- HeartOfOak ----

        [Test]
        public void HeartOfOak_OnAttach_IncreasesMaxHP()
        {
            var artifact = new HeartOfOak();
            artifact.OnAttach(_unit);

            Assert.AreEqual(125, _unit.Stats.MaxHP);
        }

        [Test]
        public void HeartOfOak_OnAttach_IncreasesCurrentHP()
        {
            var artifact = new HeartOfOak();
            artifact.OnAttach(_unit);

            Assert.AreEqual(105, _unit.Stats.CurrentHP);
        }

        [Test]
        public void HeartOfOak_OnDetach_RestoresMaxHP()
        {
            var artifact = new HeartOfOak();
            artifact.OnAttach(_unit);
            artifact.OnDetach(_unit);

            Assert.AreEqual(100, _unit.Stats.MaxHP);
        }

        [Test]
        public void HeartOfOak_OnDetach_ClampsCurrentHPToMaxHP()
        {
            var artifact = new HeartOfOak();
            artifact.OnAttach(_unit);

            _unit.Stats.CurrentHP = 125; // Manually set above original max
            artifact.OnDetach(_unit);

            Assert.LessOrEqual(_unit.Stats.CurrentHP, _unit.Stats.MaxHP);
        }

        // ---- IronHeart ----

        [Test]
        public void IronHeart_OnAttach_IncreasesArmor()
        {
            var artifact = new IronHeart();
            artifact.OnAttach(_unit);

            Assert.AreEqual(13, _unit.Stats.Armor);
        }

        [Test]
        public void IronHeart_OnDetach_RestoresArmor()
        {
            var artifact = new IronHeart();
            artifact.OnAttach(_unit);
            artifact.OnDetach(_unit);

            Assert.AreEqual(5, _unit.Stats.Armor);
        }

        // ---- SteelScales ----

        [Test]
        public void SteelScales_OnAttach_IncreasesArmor()
        {
            var artifact = new SteelScales();
            artifact.OnAttach(_unit);

            Assert.AreEqual(10, _unit.Stats.Armor);
        }

        [Test]
        public void SteelScales_OnDetach_RestoresArmor()
        {
            var artifact = new SteelScales();
            artifact.OnAttach(_unit);
            artifact.OnDetach(_unit);

            Assert.AreEqual(5, _unit.Stats.Armor);
        }

        // ---- Quickboots ----

        [Test]
        public void Quickboots_OnAttach_IncreasesSpeed()
        {
            var artifact = new Quickboots();
            artifact.OnAttach(_unit);

            Assert.AreEqual(6, _unit.Stats.Speed);
        }

        [Test]
        public void Quickboots_OnDetach_RestoresSpeed()
        {
            var artifact = new Quickboots();
            artifact.OnAttach(_unit);
            artifact.OnDetach(_unit);

            Assert.AreEqual(3, _unit.Stats.Speed);
        }
    }
}
