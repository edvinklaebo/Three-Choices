using System;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class FireballAbilityTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
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
            // Clear damage pipeline before each test
            DamagePipeline.Clear();
        }

        [Test]
        public void Fireball_DealsDamageToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var fireball = new Fireball(10);
            fireball.OnAttack(caster, target);

            Assert.Less(target.Stats.CurrentHP, 100, "Fireball should deal damage to target");
        }

        [Test]
        public void Fireball_AppliesBurnEffect()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var fireball = new Fireball(10);
            fireball.OnAttack(caster, target);

            Assert.AreEqual(1, target.StatusEffects.Count, "Fireball should apply burn effect");
            Assert.AreEqual("Burn", target.StatusEffects[0].Id, "Status effect should be Burn");
        }

        [Test]
        public void Fireball_BurnScalesWithDamageDealt()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Fireball with 10 base damage, 50% burn
            var fireball = new Fireball(10, burnDamagePercent: 0.5f);
            fireball.OnAttack(caster, target);

            if (target.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");

            Assert.AreEqual(5, burn.BaseDamage, "Burn damage should be 50% of fireball damage");
        }

        [Test]
        public void Fireball_RespectsArmor()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var tank = CreateUnit("Tank", 100, 0, 100, 5); // 100 armor = 50% reduction

            var fireball = new Fireball(20);
            var hpBefore = tank.Stats.CurrentHP;
            fireball.OnAttack(caster, tank);
            var hpAfter = tank.Stats.CurrentHP;

            var damageDealt = hpBefore - hpAfter;
            // With 100 armor: 100 / (100 + 100) = 0.5 multiplier, so 20 * 0.5 = 10 damage
            Assert.AreEqual(10, damageDealt, "Fireball should be reduced by armor");
        }

        [Test]
        public void Fireball_CanCrit()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Add 100% crit chance modifier to ensure crit
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var fireball = new Fireball(10);
            var hpBefore = target.Stats.CurrentHP;
            fireball.OnAttack(caster, target);
            var hpAfter = target.Stats.CurrentHP;

            var damageDealt = hpBefore - hpAfter;
            // With 100% crit and 2x multiplier: 10 * 2 = 20 damage
            Assert.AreEqual(20, damageDealt, "Fireball should be able to crit");
        }

        [Test]
        public void Fireball_BurnPutsOneDebuffStack()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Add 100% crit chance modifier
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var fireball = new Fireball(10, burnDamagePercent: 0.5f);
            fireball.OnAttack(caster, target);

            var burn = target.StatusEffects[0];
            Assert.AreEqual(1, burn.Stacks, "Fireball puts one burn");
        }

        [Test]
        public void Fireball_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDirectDamage(100); // Kill the target

            Assert.IsTrue(target.isDead, "Target should be dead");

            var fireball = new Fireball(10);
            fireball.OnAttack(caster, target);

            Assert.AreEqual(0, target.StatusEffects.Count, "Dead target should not receive burn");
        }

        [Test]
        public void Fireball_TriggersAtTurnStartInCombat()
        {
            var caster = CreateUnit("Caster", 100, 50, 0, 10);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Add fireball ability
            caster.Abilities.Add(new Fireball(15));

            CombatSystem.RunFight(caster, target);

            // Target should have taken fireball damage + regular attack damage + burn over time
            Assert.IsTrue(target.isDead, "Target should be dead from fireball + attacks + burn");
            Assert.Greater(caster.Stats.CurrentHP, 0, "Caster should survive (target has 0 attack)");
        }

        [Test]
        public void Fireball_TriggersBeforeAttack()
        {
            var caster = CreateUnit("Caster", 100, 10, 0, 10);
            var target = CreateUnit("Target", 30, 0, 0, 5);

            // Add fireball that should kill or severely damage target
            caster.Abilities.Add(new Fireball(20));

            var actions = CombatSystem.RunFight(caster, target);

            // Verify that fireball action happens before damage action in first round
            var firstFireball = -1;
            var firstAttack = -1;

            for (var i = 0; i < actions.Count; i++)
            {
                if (actions[i] is FireballAction && firstFireball == -1)
                    firstFireball = i;
                if (actions[i] is DamageAction && firstAttack == -1)
                    firstAttack = i;
            }

            Assert.Greater(firstFireball, -1, "Should have fireball action");
            Assert.Greater(firstAttack, -1, "Should have attack action");
            Assert.Less(firstFireball, firstAttack, "Fireball should happen before attack");
        }

        [Test]
        public void Fireball_BurnTicksOnTargetsTurn()
        {
            var caster = CreateUnit("Caster", 200, 5, 0, 10);
            var target = CreateUnit("Target", 50, 5, 0, 5);

            // Add fireball ability to caster
            caster.Abilities.Add(new Fireball());

            var actions = CombatSystem.RunFight(caster, target);

            // Look for StatusEffectAction for burn
            var burnActions = 0;
            foreach (var action in actions)
                if (action is StatusEffectAction statusAction && statusAction.EffectName == "Burn")
                    burnActions++;

            Assert.Greater(burnActions, 0, "Burn should tick at least once during combat");
        }

        [Test]
        public void Fireball_WithMultipleApplications_BurnKeepsHighestDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var fireball = new Fireball();

            // First application
            fireball.OnAttack(caster, target);
            var firstBurnDamage = target.StatusEffects[0].Stacks;

            // Second application with higher base damage
            var strongerFireball = new Fireball(20, burnDamagePercent: 0.5f);
            strongerFireball.OnAttack(caster, target);

            if (target.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");

            Assert.AreEqual(1, target.StatusEffects.Count, "Should still have only 1 burn effect");
            Assert.Greater(burn.BaseDamage, firstBurnDamage, "Burn should update to higher damage");
        }
    }
}