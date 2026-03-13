using System.Linq;

using Core;
using Core.Artifacts.Passives;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for ICombatListener artifact passives: VampiricFang, ThornArmor, TwinBlades.
    /// These passives are registered by CombatEngine from Unit.Artifacts.
    /// </summary>
    public class CombatListenerArtifactTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
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

        // ---- VampiricFang ----

        [Test]
        public void VampiricFang_HealsAttacker_WhenDamageIsDealt()
        {
            var attacker = CreateUnit("Vampire", 50, 10, 0, 10);
            attacker.Stats.CurrentHP = 30; // Reduce HP to test healing

            var defender = CreateUnit("Victim", 100, 0, 0, 5);

            var artifact = new VampiricFang(0.2f);
            artifact.OnAttach(attacker);
            attacker.Artifacts.Add(artifact);

            var initialHP = attacker.Stats.CurrentHP;

            CombatSystem.RunFight(attacker, defender);

            Assert.Greater(attacker.Stats.CurrentHP, initialHP, "VampiricFang should heal the attacker");
        }

        [Test]
        public void VampiricFang_QueuesHealAction_InCombat()
        {
            var attacker = CreateUnit("Vampire", 100, 20, 0, 10);
            attacker.Stats.CurrentHP = 80;
            var defender = CreateUnit("Victim", 100, 0, 0, 5);

            var artifact = new VampiricFang(0.2f);
            artifact.OnAttach(attacker);
            attacker.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "VampiricFang should queue HealAction in combat");
        }

        // ---- ThornArmor ----

        [Test]
        public void ThornArmor_ReflectsDamage_WhenHit()
        {
            var defender = CreateUnit("Tank", 100, 0, 10, 5); // Armor = 10
            var attacker = CreateUnit("Attacker", 200, 5, 0, 10);

            var artifact = new ThornArmor();
            artifact.OnAttach(defender);
            defender.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            // ThornArmor should create ThornsAction entries in the log
            var thornsActions = actions.OfType<ThornsAction>().ToList();
            Assert.IsNotEmpty(thornsActions, "ThornArmor should create ThornsAction entries");
        }

        [Test]
        public void ThornArmor_ReflectsDamage_EqualToHalfArmor()
        {
            var defender = CreateUnit("Tank", 500, 0, 10, 5); // Armor = 10, half = 5
            var attacker = CreateUnit("Attacker", 200, 5, 0, 10);

            var artifact = new ThornArmor();
            artifact.OnAttach(defender);
            defender.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            var thornsAction = actions.OfType<ThornsAction>().First();
            Assert.AreEqual(5, thornsAction.Amount, "ThornArmor should reflect half armor (10/2 = 5)");
        }

        [Test]
        public void ThornArmor_NoReflect_WhenArmorIsZero()
        {
            var defender = CreateUnit("Tank", 100, 0, 0, 5); // Armor = 0
            var attacker = CreateUnit("Attacker", 200, 5, 0, 10);

            var artifact = new ThornArmor();
            artifact.OnAttach(defender);
            defender.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            var thornsActions = actions.OfType<ThornsAction>().ToList();
            Assert.IsEmpty(thornsActions, "ThornArmor should not reflect when armor is 0");
        }

        // ---- TwinBlades ----

        [Test]
        public void TwinBlades_WithAlwaysTrigger_ExecutesExtraStrike()
        {
            var attacker = CreateUnit("Fighter", 100, 10, 0, 10);
            var defender = CreateUnit("Victim", 500, 0, 0, 5); // High HP so combat continues

            // 100% trigger chance
            var artifact = new TwinBlades(triggerChance: 1.0f, damageMultiplier: 0.75f);
            artifact.OnAttach(attacker);
            attacker.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            // With 100% trigger chance, every attack should have an extra hit
            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.Greater(damageActions.Count, 1, "TwinBlades should produce more than one damage action");
        }

        [Test]
        public void TwinBlades_WithNeverTrigger_NoExtraStrike()
        {
            var attacker = CreateUnit("Fighter", 100, 10, 0, 10);
            var defender = CreateUnit("Victim", 20, 0, 0, 5);

            // 0% trigger chance — no extra hits expected
            var artifact = new TwinBlades(triggerChance: 0.0f, damageMultiplier: 0.75f);
            artifact.OnAttach(attacker);
            attacker.Artifacts.Add(artifact);

            var actions = CombatSystem.RunFight(attacker, defender);

            // With 0% trigger chance, attacker should have exactly 2 normal strikes
            // (defender has 20 HP, attacker deals 10 per hit)
            var attackerDamageActions = actions.OfType<DamageAction>().Where(a => a.Source == attacker).ToList();
            Assert.AreEqual(2, attackerDamageActions.Count, "TwinBlades at 0% chance should not add extra damage actions from the attacker");
        }
    }
}
