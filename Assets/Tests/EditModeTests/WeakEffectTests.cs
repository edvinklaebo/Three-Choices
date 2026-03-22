using Core;
using Core.StatusEffects;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    public class WeakEffectTests
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

        // ---- Basic application ----

        [Test]
        public void Weak_ReducesOutgoingDamage_BySingleStack()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.ApplyStatus(new Weak(1, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(9, ctx.FinalValue, "1 stack of Weak should reduce damage by 1");
        }

        [Test]
        public void Weak_ReducesOutgoingDamage_ByMultipleStacks()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.ApplyStatus(new Weak(5, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(5, ctx.FinalValue, "5 stacks of Weak should reduce damage by 5");
        }

        [Test]
        public void Weak_DamageNeverGoesBelowZero()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // 20 stacks reduces 1-damage attack to below zero — should clamp at 0
            attacker.ApplyStatus(new Weak(20, 3, damageReductionPerStack: 1, maxStacks: 20));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 1);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(0, ctx.FinalValue, "Final damage should never go below 0");
        }

        [Test]
        public void Weak_DoesNotDealTickDamage()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Weak(3, 3));

            unit.TickStatusesTurnStart();

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Weak should not deal any tick damage");
        }

        // ---- Duration ----

        [Test]
        public void Weak_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(1, 3);
            unit.ApplyStatus(weak);

            Assert.AreEqual(3, weak.Duration);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, weak.Duration);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, weak.Duration);
        }

        [Test]
        public void Weak_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Weak(1, 2));

            Assert.AreEqual(1, unit.StatusEffects.Count);

            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(0, unit.StatusEffects.Count, "Weak should expire after duration reaches 0");
        }

        [Test]
        public void Weak_NoLongerReducesDamageAfterExpiry()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.ApplyStatus(new Weak(5, 1));

            // Tick once to expire (duration=1 -> 0)
            attacker.TickStatusesTurnStart();

            Assert.AreEqual(0, attacker.StatusEffects.Count, "Weak should have expired");

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Expired Weak should no longer reduce damage");
        }

        // ---- Stacking ----

        [Test]
        public void Weak_StacksAccumulateOnReapply()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak1 = new Weak(2, 3);
            var weak2 = new Weak(3, 3);

            unit.ApplyStatus(weak1);
            unit.ApplyStatus(weak2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have one Weak effect");
            Assert.AreEqual(5, weak1.Stacks, "Stacks should accumulate: 2 + 3 = 5");
        }

        [Test]
        public void Weak_StacksCapAtMaxStacks()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Weak(5, 3, maxStacks: 6));
            unit.ApplyStatus(new Weak(5, 3, maxStacks: 6));

            Assert.AreEqual(6, unit.StatusEffects[0].Stacks, "Stacks should be capped at maxStacks=6");
        }

        [Test]
        public void Weak_RefreshesDurationOnReapply_WhenEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(1, 3, refreshDurationOnReapply: true);
            unit.ApplyStatus(weak);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, weak.Duration);

            // Reapply — duration should refresh to base (3)
            unit.ApplyStatus(new Weak(1, 3, refreshDurationOnReapply: true));
            Assert.AreEqual(3, weak.Duration, "Duration should refresh to base value on reapply");
        }

        [Test]
        public void Weak_DoesNotRefreshDurationOnReapply_WhenDisabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(1, 3, refreshDurationOnReapply: false);
            unit.ApplyStatus(weak);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, weak.Duration);

            // Reapply with refreshDurationOnReapply=false — duration should NOT change
            unit.ApplyStatus(new Weak(1, 3, refreshDurationOnReapply: false));
            Assert.AreEqual(2, weak.Duration, "Duration should not refresh when refreshDurationOnReapply=false");
        }

        // ---- Stack decay ----

        [Test]
        public void Weak_StacksDecayEachTurn_WhenDecayEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(6, 10, stackDecayPerTurn: 2);
            unit.ApplyStatus(weak);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(4, weak.Stacks, "Stacks should decay by 2 per turn");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, weak.Stacks);
        }

        [Test]
        public void Weak_StacksDoNotGoBelowZero_WhenDecayEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(1, 10, stackDecayPerTurn: 5);
            unit.ApplyStatus(weak);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(0, weak.Stacks, "Stacks should not go below 0 from decay");
        }

        [Test]
        public void Weak_NoStackDecay_WhenDecayIsZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var weak = new Weak(5, 10, stackDecayPerTurn: 0);
            unit.ApplyStatus(weak);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(5, weak.Stacks, "Stacks should not decay when stackDecayPerTurn=0");
        }

        // ---- Custom reduction per stack ----

        [Test]
        public void Weak_CustomDamageReductionPerStack()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.ApplyStatus(new Weak(3, 3, damageReductionPerStack: 2));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(4, ctx.FinalValue, "3 stacks × 2 reduction = 6 total reduction (10 - 6 = 4)");
        }

        // ---- ScriptableObject definition constructor ----

        [Test]
        public void Weak_DataDrivenConstructor_ReadsAllValuesFromDefinition()
        {
            var definition = UnityEngine.ScriptableObject.CreateInstance<WeakDefinition>();
            definition.EditorInit(
                stacks: 2,
                duration: 4,
                damageReductionPerStack: 1,
                maxStacks: 8,
                stackDecayPerTurn: 1,
                refreshDurationOnReapply: true
            );

            var weak = new Weak(definition);

            Assert.AreEqual(2, weak.Stacks);
            Assert.AreEqual(4, weak.Duration);
            Assert.AreEqual(0, weak.BaseDamage, "BaseDamage should always be 0 for Weak");
            Assert.AreEqual("Weak", weak.Id);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        // ---- Integration: combat ----

        [Test]
        public void Weak_ReducesDamageInCombat()
        {
            var attacker = CreateUnit("Weakened", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            attacker.ApplyStatus(new Weak(5, 5));

            CombatSystem.RunFight(attacker, defender);

            // Attacker has 10 attack - 5 weak = 5 effective damage per turn
            // Defender has 100 HP, 0 attack — defender should eventually die but take fewer hits
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should eventually die");
            Assert.AreEqual(100, attacker.Stats.CurrentHP, "Attacker takes no damage from 0-attack defender");
        }

        [Test]
        public void Weak_CanCoexistWithOtherStatusEffects()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Weak(3, 3));
            unit.ApplyStatus(new Bleed(2, 3, 1));

            Assert.AreEqual(2, unit.StatusEffects.Count, "Should have both Weak and Bleed");
        }
    }
}
