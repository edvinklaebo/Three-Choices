using System.Linq;

using Core;
using Core.Combat;
using Core.Modifiers;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="ArmorMitigationModifier"/>.
    /// Verifies the formula: modifier = 100 / (100 + armor), applied during the Mitigation phase.
    /// </summary>
    public class ArmorMitigationTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed = 10)
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

        // ---- Direct modifier via CombatContext ----

        [Test]
        public void ArmorMitigation_ZeroArmor_DamageUnchanged()
        {
            var source = CreateUnit("Attacker", 100, 10, 0);
            var target = CreateUnit("Defender", 100, 0, armor: 0);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // 100 / (100 + 0) = 1.0, no reduction
            Assert.AreEqual(90, target.Stats.CurrentHP, "Zero armor should not reduce damage");
        }

        [Test]
        public void ArmorMitigation_HundredArmor_HalvesDamage()
        {
            var source = CreateUnit("Attacker", 100, 10, 0);
            var target = CreateUnit("Defender", 100, 0, armor: 100);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // 100 / (100 + 100) = 0.5, CeilToInt(10 * 0.5) = 5
            Assert.AreEqual(95, target.Stats.CurrentHP, "100 armor should halve damage (10 -> 5)");
        }

        [Test]
        public void ArmorMitigation_FourHundredArmor_ReducesDamageToTwentyPercent()
        {
            var source = CreateUnit("Attacker", 100, 10, 0);
            var target = CreateUnit("Defender", 100, 0, armor: 400);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // 100 / (100 + 400) = 0.2, CeilToInt(10 * 0.2) = 2
            Assert.AreEqual(98, target.Stats.CurrentHP, "400 armor should reduce damage to 20% (10 -> 2)");
        }

        [Test]
        public void ArmorMitigation_RoundsUpResidualDamage()
        {
            var source = CreateUnit("Attacker", 100, 10, 0);
            var target = CreateUnit("Defender", 100, 0, armor: 50);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // 100 / (100 + 50) ≈ 0.6667, CeilToInt(10 * 0.6667) = CeilToInt(6.667) = 7
            Assert.AreEqual(93, target.Stats.CurrentHP, "Armor mitigation should round up (10 * 0.667 = 6.67 -> 7)");
        }

        [Test]
        public void ArmorMitigation_NeverReducesDamageToZero_ForPositiveBaseDamage()
        {
            var source = CreateUnit("Attacker", 100, 10, 0);
            var target = CreateUnit("Defender", 100, 0, armor: 9999);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // Even with massive armor, CeilToInt(10 * 100/10099) = CeilToInt(0.099) = 1
            Assert.AreEqual(99, target.Stats.CurrentHP, "Even maximum armor should leave at least 1 damage (rounded up)");
        }

        [Test]
        public void ArmorMitigation_DoesNotApplyToSource_OnlyToTarget()
        {
            var source = CreateUnit("Attacker", 100, 10, armor: 100); // source has armor
            var target = CreateUnit("Defender", 100, 0, armor: 0);    // target has no armor
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());

            context.DealDamage(source, target, 10);

            // Target has 0 armor — no reduction
            Assert.AreEqual(90, target.Stats.CurrentHP, "Armor on source should not reduce damage dealt to target");
        }

        // ---- CombatEngine integration ----

        [Test]
        public void CombatEngine_ArmorReducesDamagePerHit()
        {
            // Attacker deals 100 damage per hit; target has 100 armor (50% reduction = 50 damage/hit)
            var attacker = CreateUnit("Attacker", 200, 100, armor: 0, speed: 10);
            var target = CreateUnit("Target_Armored", 200, 0, armor: 100, speed: 5);

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, target);

            var damageActions = actions.OfType<DamageAction>()
                .Where(a => a.Target == target)
                .ToList();

            Assert.IsTrue(damageActions.All(a => a.Amount <= 50),
                "With 100 armor each hit should be at most 50 damage (CeilToInt(100 * 0.5) = 50)");
        }

        [Test]
        public void CombatEngine_ArmoredUnitSurvivesLonger_ThanUnarmoredUnit()
        {
            // Attacker deals 20 damage per hit
            // Without armor: defender (40 HP) dies in 2 hits
            // With 100 armor: modifier = 0.5, damage per hit = 10, dies in 4 hits
            var attackerNoArmor = CreateUnit("Attacker", 200, 20, 0, 10);
            var defenderNoArmor = CreateUnit("DefNoArmor", 40, 0, 0, 5);

            var attackerWithArmor = CreateUnit("Attacker2", 200, 20, 0, 10);
            var defenderWithArmor = CreateUnit("DefArmor", 40, 0, 100, 5);

            var actionsNoArmor = CombatSystem.RunFight(attackerNoArmor, defenderNoArmor);
            var actionsWithArmor = CombatSystem.RunFight(attackerWithArmor, defenderWithArmor);

            var hitsToKillNoArmor = actionsNoArmor
                .OfType<DamageAction>()
                .Count(a => a.Source == attackerNoArmor);
            var hitsToKillWithArmor = actionsWithArmor
                .OfType<DamageAction>()
                .Count(a => a.Source == attackerWithArmor);

            Assert.Greater(hitsToKillWithArmor, hitsToKillNoArmor,
                "Armored defender should require more hits to kill");
        }
    }
}
