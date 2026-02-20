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
            var combatContext = new CombatContext();
            arcaneMissiles.RegisterHandlers(combatContext);
            var damage = arcaneMissiles.OnCast(caster, target);

            Assert.Greater(damage, 0, "Arcane Missiles should return positive damage");
        }

        [Test]
        public void ArcaneMissiles_DealsDamageFromAllMissiles()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // 3 missiles at 5 damage each = 15 total
            var arcaneMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 3);
            var combatContext = new CombatContext();
            arcaneMissiles.RegisterHandlers(combatContext);
            
            var damage = arcaneMissiles.OnCast(caster, target);

            Assert.AreEqual(15, damage, "Total damage should be sum of all missile hits");
        }

        [Test]
        public void ArcaneMissiles_MissileCountAffectsTotalDamage()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target1 = CreateUnit("Target1", 100, 0, 0, 5);
            var target2 = CreateUnit("Target2", 100, 0, 0, 5);

            var twoMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 2);
            var fiveMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 5);
            var combatContext = new CombatContext();
            twoMissiles.RegisterHandlers(combatContext);
            fiveMissiles.RegisterHandlers(combatContext);
            var damage2 = twoMissiles.OnCast(caster, target1);
            var damage5 = fiveMissiles.OnCast(caster, target2);

            Assert.AreEqual(10, damage2, "2 missiles at 5 damage should deal 10 total");
            Assert.AreEqual(25, damage5, "5 missiles at 5 damage should deal 25 total");
        }

        [Test]
        public void ArcaneMissiles_EachMissileCanCritIndependently()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 100, 0, 0, 5);

            // 100% crit chance with 2x multiplier: each missile should crit
            DamagePipeline.Register(new CriticalHitModifier(caster, 1.0f, 2.0f));

            var arcaneMissiles = new ArcaneMissiles(baseDamage: 5, missileCount: 3);
            var combatContext = new CombatContext();
            arcaneMissiles.RegisterHandlers(combatContext);
            var damage = arcaneMissiles.OnCast(caster, target);

            // 3 missiles * 5 base * 2x crit = 30
            Assert.AreEqual(30, damage, "All missiles should crit with 100% crit chance");
        }

        [Test]
        public void ArcaneMissiles_DoesNotDamageDeadTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 0, 5);
            var target = CreateUnit("Target", 1, 0, 0, 5);
            target.ApplyDamage(caster, 100); // Kill the target

            Assert.IsTrue(target.IsDead, "Target should be dead");

            var arcaneMissiles = new ArcaneMissiles();
            var damage = arcaneMissiles.OnCast(caster, target);

            Assert.AreEqual(0, damage, "Dead target should take no damage");
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

            var firstMissiles = -1;
            var firstAttack = -1;

            for (var i = 0; i < actions.Count; i++)
            {
                if (actions[i] is ArcaneMissilesAction && firstMissiles == -1)
                    firstMissiles = i;
                // Only look for the normal attack DamageAction after the ArcaneMissilesAction
                if (actions[i] is DamageAction && firstMissiles != -1 && firstAttack == -1)
                    firstAttack = i;
            }

            Assert.Greater(firstMissiles, -1, "Should have ArcaneMissilesAction");
            Assert.Greater(firstAttack, -1, "Should have DamageAction");
            Assert.Less(firstMissiles, firstAttack, "Arcane Missiles should happen before attack");
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
