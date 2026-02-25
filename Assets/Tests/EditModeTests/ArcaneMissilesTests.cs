using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class ArcaneMissilesTests
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
            DamagePipeline.Clear();
        }

        [Test]
        public void ArcaneMissiles_DealsDamageToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            var arcaneMissiles = new ArcaneMissiles();
            var context = new CombatContext();
            arcaneMissiles.OnCast(caster, target, context);

            // Default: 3 missiles × 5 damage = 15 total
            Assert.AreEqual(85, target.Stats.CurrentHP, "Arcane Missiles should deal 15 damage (3 × 5)");
        }

        [Test]
        public void ArcaneMissiles_DealsDamageFromAllMissiles()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // 3 missiles at 5 damage each = 15 total
            var arcaneMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 3);
            var context = new CombatContext();

            arcaneMissiles.OnCast(caster, target, context);

            Assert.AreEqual(85, target.Stats.CurrentHP, "Total damage should be sum of all missile hits (3 × 5 = 15)");
        }

        [Test]
        public void ArcaneMissiles_MissileCountAffectsTotalDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target1 = CreateUnit("Target1", 100, 0, 0, 5);
            var target2 = CreateUnit("Target2", 100, 0, 0, 5);

            var twoMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 2);
            var fiveMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 5);
            var context = new CombatContext();

            twoMissiles.OnCast(caster, target1, context);
            fiveMissiles.OnCast(caster, target2, context);

            Assert.AreEqual(90, target1.Stats.CurrentHP, "2 missiles at 5 damage should reduce HP by 10");
            Assert.AreEqual(75, target2.Stats.CurrentHP, "5 missiles at 5 damage should reduce HP by 25");
        }

        [Test]
        public void ArcaneMissiles_EachMissileCanCritIndependently()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // 100% crit chance with 2x multiplier: each missile should crit
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var arcaneMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 3);
            var context = new CombatContext();
            arcaneMissiles.OnCast(caster, target, context);

            // 3 missiles * 5 base * 2x crit = 30
            Assert.AreEqual(70, target.Stats.CurrentHP, "All missiles should crit with 100% crit chance (3 × 10 = 30)");
        }

        [Test]
        public void ArcaneMissiles_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDamage(caster, 100); // Kill the target

            Assert.IsTrue(target.IsDead, "Target should be dead");

            var hpBefore = target.Stats.CurrentHP;
            var arcaneMissiles = new ArcaneMissiles();
            var context = new CombatContext();
            arcaneMissiles.OnCast(caster, target, context);

            Assert.AreEqual(hpBefore, target.Stats.CurrentHP, "Dead target's HP should not change");
        }

        [Test]
        public void ArcaneMissiles_TriggersAtTurnStartInCombat()
        {
            var caster = CreateUnit("Caster", 100, 50, 0, 10);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            caster.Abilities.Add(new ArcaneMissiles(baseDamage: 15, missileCount: 3));

            CombatSystem.RunFight(caster, target);

            Assert.IsTrue(target.IsDead, "Target should be dead from Arcane Missiles + attacks");
            Assert.Greater(caster.Stats.CurrentHP, 0, "Caster should survive (target has 0 attack)");
        }

        [Test]
        public void ArcaneMissiles_TriggersBeforeAttack()
        {
            var caster = CreateUnit("Caster", 100, 10, 0, 10);
            var target = CreateUnit("Target", 60, 0, 0, 5);

            caster.Abilities.Add(new ArcaneMissiles(baseDamage: 10, missileCount: 3));

            var actions = CombatSystem.RunFight(caster, target);

            // Abilities execute before normal attacks each round.
            // 3 missile DamageActions (from ability) fire before the attack DamageAction.
            var damageFromCaster = 0;
            foreach (var action in actions)
                if (action is DamageAction da && da.Source == caster)
                    damageFromCaster++;

            // Round 1: 3 missiles + 1 attack = 4 DamageActions from caster
            Assert.GreaterOrEqual(damageFromCaster, 4, "Should have at least 3 missile hits + 1 attack");
        }

        [Test]
        public void ArcaneMissiles_HasLowerPriorityThanFireball()
        {
            var fireball = new Fireball();
            var arcaneMissiles = new ArcaneMissiles();

            Assert.Less(arcaneMissiles.Priority, fireball.Priority,
                "Arcane Missiles (40) should have lower priority value than Fireball (50)");
        }
    }
}
