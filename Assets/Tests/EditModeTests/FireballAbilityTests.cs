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

            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

            // Fireball default: 10 base damage, target has 0 armor → 10 damage dealt
            Assert.AreEqual(90, target.Stats.CurrentHP, "Fireball should deal 10 damage to the target");
        }

        [Test]
        public void Fireball_AppliesBurnEffect()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

            Assert.AreEqual(1, target.StatusEffects.Count, "Fireball should apply burn effect");
            Assert.AreEqual("Burn", target.StatusEffects[0].Id, "Status effect should be Burn");
        }

        [Test]
        public void Fireball_BurnScalesWithDamageDealt()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Fireball with 10 base damage, 50% burn
            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

            if (target.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");

            Assert.AreEqual(5, burn.BaseDamage, "Burn damage should be 50% of fireball damage");
        }

        [Test]
        public void Fireball_ShouldRespectArmor()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var tank = CreateUnit("Tank", 100, 0, 100, 5); // 100 armor

            var fireball = new Fireball(20);
            var context = new CombatContext();
            context.RegisterListener(new ArmorMitigationModifier());
            fireball.OnCast(caster, tank, context);

            // Formula: 100 / (100 + 100) = 0.5 → CeilToInt(20 * 0.5) = 10 damage
            Assert.AreEqual(90, tank.Stats.CurrentHP, "Fireball damage should be reduced by armor");
        }

        [Test]
        public void Fireball_CanCrit()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Add 100% crit chance modifier to ensure crit
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

            // With 100% crit and 2x multiplier: 10 * 2 = 20 damage → HP = 80
            Assert.AreEqual(80, target.Stats.CurrentHP, "Fireball should be able to crit (10 × 2 = 20 damage)");
        }

        [Test]
        public void Fireball_BurnPutsOneDebuffStack()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // Add 100% crit chance modifier
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

            var burn = target.StatusEffects[0];
            Assert.AreEqual(1, burn.Stacks, "Fireball puts one burn");
        }

        [Test]
        public void Fireball_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDamage(caster, 100); // Kill the target

            Assert.IsTrue(target.IsDead, "Target should be dead");

            var fireball = new Fireball();
            var context = new CombatContext();
            fireball.OnCast(caster, target, context);

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
            Assert.IsTrue(target.IsDead, "Target should be dead from fireball + attacks + burn");
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

            // Abilities fire before the normal attack each round.
            // Fireball(20) + attack(10) vs 30 HP: both happen in round 1.
            // We expect at least 2 DamageActions from the caster (fireball + attack).
            var damageActionsFromCaster = 0;
            foreach (var action in actions)
                if (action is DamageAction da && da.Source == caster)
                    damageActionsFromCaster++;

            Assert.GreaterOrEqual(damageActionsFromCaster, 2, "Should have at least 1 fireball hit + 1 attack");
        }

        [Test]
        public void Fireball_BurnTicksOnTargetsTurn()
        {
            var caster = CreateUnit("Caster", 200, 5, 0, 10);
            var target = CreateUnit("Target", 50, 5, 0, 5);

            caster.Abilities.Add(new Fireball());

            var actions = CombatSystem.RunFight(caster, target);

            var burnActions = 0;
            foreach (var action in actions)
                if (action is StatusEffectAction { EffectName: "Burn" })
                    burnActions++;

            Assert.Greater(burnActions, 0, "Burn should tick at least once during combat");
        }

        [Test]
        public void Fireball_WithMultipleApplications_BurnKeepsHighestDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 200, 0, 0, 5);

            var fireball = new Fireball();
            var context = new CombatContext();

            // First application
            fireball.OnCast(caster, target, context);
            var firstBurnDamage = target.StatusEffects[0].Stacks;

            // Second application with higher base damage
            var strongerFireball = new Fireball(20, burnDamagePercent: 0.5f);
            strongerFireball.OnCast(caster, target, context);

            if (target.StatusEffects[0] is not Burn burn)
                throw new Exception("Burn should be applied to target");

            Assert.AreEqual(1, target.StatusEffects.Count, "Should still have only 1 burn effect");
            Assert.Greater(burn.BaseDamage, firstBurnDamage, "Burn should update to higher damage");
        }
    }
}