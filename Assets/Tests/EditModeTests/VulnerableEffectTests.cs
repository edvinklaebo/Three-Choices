using Core;
using Core.StatusEffects;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    public class VulnerableEffectTests
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
        public void Vulnerable_IncreasesIncomingDamage_BySingleStack()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            defender.ApplyStatus(new Vulnerable(1, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(11, ctx.FinalValue, "1 stack of Vulnerable should increase damage by 1");
        }

        [Test]
        public void Vulnerable_IncreasesIncomingDamage_ByMultipleStacks()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            defender.ApplyStatus(new Vulnerable(5, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(15, ctx.FinalValue, "5 stacks of Vulnerable should increase damage by 5");
        }

        [Test]
        public void Vulnerable_DoesNotAffectAttackerOutgoingDamage()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            // Vulnerable is on attacker — should not affect damage dealt by attacker
            attacker.ApplyStatus(new Vulnerable(5, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Vulnerable on the attacker should not affect outgoing damage");
        }

        [Test]
        public void Vulnerable_OnAttacker_StillIncreasesIncomingDamageWhenAttackerIsHit()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var enemy = CreateUnit("Enemy", 100, 10, 0, 5);

            // Vulnerable is on attacker — when THEY are hit, they take more damage
            attacker.ApplyStatus(new Vulnerable(5, 3));

            DamagePipeline.Clear();
            var ctx = new DamageContext(enemy, attacker, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(15, ctx.FinalValue, "Vulnerable on the unit being hit should increase damage taken");
        }

        [Test]
        public void Vulnerable_DoesNotDealTickDamage()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Vulnerable(3, 3));

            unit.TickStatusesTurnStart();

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Vulnerable should not deal any tick damage");
        }

        // ---- Duration ----

        [Test]
        public void Vulnerable_ReducesDurationEachTick()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(1, 3);
            unit.ApplyStatus(vulnerable);

            Assert.AreEqual(3, vulnerable.Duration);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, vulnerable.Duration);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(1, vulnerable.Duration);
        }

        [Test]
        public void Vulnerable_ExpiresWhenDurationReachesZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Vulnerable(1, 2));

            Assert.AreEqual(1, unit.StatusEffects.Count);

            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(0, unit.StatusEffects.Count, "Vulnerable should expire after duration reaches 0");
        }

        [Test]
        public void Vulnerable_NoLongerIncreasesDamageAfterExpiry()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            defender.ApplyStatus(new Vulnerable(5, 1));

            // Tick once to expire (duration=1 -> 0)
            defender.TickStatusesTurnStart();

            Assert.AreEqual(0, defender.StatusEffects.Count, "Vulnerable should have expired");

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(10, ctx.FinalValue, "Expired Vulnerable should no longer increase damage");
        }

        // ---- Stacking ----

        [Test]
        public void Vulnerable_StacksAccumulateOnReapply()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vuln1 = new Vulnerable(2, 3);
            var vuln2 = new Vulnerable(3, 3);

            unit.ApplyStatus(vuln1);
            unit.ApplyStatus(vuln2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have one Vulnerable effect");
            Assert.AreEqual(5, vuln1.Stacks, "Stacks should accumulate: 2 + 3 = 5");
        }

        [Test]
        public void Vulnerable_StacksCapAtMaxStacks()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Vulnerable(5, 3, maxStacks: 6));
            unit.ApplyStatus(new Vulnerable(5, 3, maxStacks: 6));

            Assert.AreEqual(6, unit.StatusEffects[0].Stacks, "Stacks should be capped at maxStacks=6");
        }

        [Test]
        public void Vulnerable_RefreshesDurationOnReapply_WhenEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(1, 3, refreshDurationOnReapply: true);
            unit.ApplyStatus(vulnerable);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, vulnerable.Duration);

            // Reapply — duration should refresh to base (3)
            unit.ApplyStatus(new Vulnerable(1, 3, refreshDurationOnReapply: true));
            Assert.AreEqual(3, vulnerable.Duration, "Duration should refresh to base value on reapply");
        }

        [Test]
        public void Vulnerable_DoesNotRefreshDurationOnReapply_WhenDisabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(1, 3, refreshDurationOnReapply: false);
            unit.ApplyStatus(vulnerable);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, vulnerable.Duration);

            // Reapply — duration should NOT change
            unit.ApplyStatus(new Vulnerable(1, 3, refreshDurationOnReapply: false));
            Assert.AreEqual(2, vulnerable.Duration, "Duration should not refresh when refreshDurationOnReapply=false");
        }

        // ---- Stack decay ----

        [Test]
        public void Vulnerable_StacksDecayEachTurn_WhenDecayEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(6, 10, stackDecayPerTurn: 2);
            unit.ApplyStatus(vulnerable);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(4, vulnerable.Stacks, "Stacks should decay by 2 per turn");

            unit.TickStatusesTurnStart();
            Assert.AreEqual(2, vulnerable.Stacks);
        }

        [Test]
        public void Vulnerable_StacksDoNotGoBelowZero_WhenDecayEnabled()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(1, 10, stackDecayPerTurn: 5);
            unit.ApplyStatus(vulnerable);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(0, vulnerable.Stacks, "Stacks should not go below 0 from decay");
        }

        [Test]
        public void Vulnerable_NoStackDecay_WhenDecayIsZero()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            var vulnerable = new Vulnerable(5, 10, stackDecayPerTurn: 0);
            unit.ApplyStatus(vulnerable);

            unit.TickStatusesTurnStart();
            Assert.AreEqual(5, vulnerable.Stacks, "Stacks should not decay when stackDecayPerTurn=0");
        }

        // ---- Custom increase per stack ----

        [Test]
        public void Vulnerable_CustomDamageIncreasePerStack()
        {
            var attacker = CreateUnit("Attacker", 100, 10, 0, 5);
            var defender = CreateUnit("Defender", 100, 0, 0, 5);

            defender.ApplyStatus(new Vulnerable(3, 3, damageIncreasePerStack: 2));

            DamagePipeline.Clear();
            var ctx = new DamageContext(attacker, defender, 10);
            DamagePipeline.Process(ctx);

            Assert.AreEqual(16, ctx.FinalValue, "3 stacks × 2 increase = 6 total increase (10 + 6 = 16)");
        }

        // ---- ScriptableObject definition constructor ----

        [Test]
        public void Vulnerable_DataDrivenConstructor_ReadsAllValuesFromDefinition()
        {
            var definition = UnityEngine.ScriptableObject.CreateInstance<VulnerableDefinition>();
            definition.EditorInit(
                stacks: 2,
                duration: 4,
                damageIncreasePerStack: 1,
                maxStacks: 8,
                stackDecayPerTurn: 1,
                refreshDurationOnReapply: true
            );

            var vulnerable = new Vulnerable(definition);

            Assert.AreEqual(2, vulnerable.Stacks);
            Assert.AreEqual(4, vulnerable.Duration);
            Assert.AreEqual(0, vulnerable.BaseDamage, "BaseDamage should always be 0 for Vulnerable");
            Assert.AreEqual("Vulnerable", vulnerable.Id);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        // ---- Integration: combat ----

        [Test]
        public void Vulnerable_IncreasesDamageTakenInCombat()
        {
            var attacker = CreateUnit("Attacker", 100, 5, 0, 5);
            var defender = CreateUnit("Vulnerable Target", 100, 0, 0, 5);

            defender.ApplyStatus(new Vulnerable(5, 10));

            CombatSystem.RunFight(attacker, defender);

            // Attacker deals 5 + 5 (vulnerable) = 10 per turn; defender dies faster
            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should die faster due to Vulnerable");
            Assert.AreEqual(100, attacker.Stats.CurrentHP, "Attacker takes no damage from 0-attack defender");
        }

        [Test]
        public void Vulnerable_CanCoexistWithOtherStatusEffects()
        {
            var unit = CreateUnit("Test", 100, 0, 0, 5);
            unit.ApplyStatus(new Vulnerable(3, 3));
            unit.ApplyStatus(new Weak(3, 3));

            Assert.AreEqual(2, unit.StatusEffects.Count, "Should have both Vulnerable and Weak");
        }
    }
}
