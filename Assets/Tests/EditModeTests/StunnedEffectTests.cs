using Core;
using Core.Combat;
using Core.StatusEffects;

using Interfaces;

using NUnit.Framework;

using Systems;

namespace Tests.EditModeTests
{
    public class StunnedEffectTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = 0,
                    Speed = speed
                }
            };
        }

        // ---- ConsumeTurnSkip behaviour ----

        [Test]
        public void Stunned_ConsumeTurnSkip_ReturnsTrueAndDecrementsStack()
        {
            var stunned = new Stunned(2);

            var result = stunned.ConsumeTurnSkip();

            Assert.IsTrue(result, "ConsumeTurnSkip should return true when stacks remain");
            Assert.AreEqual(1, stunned.Stacks, "Stacks should decrement by 1");
        }

        [Test]
        public void Stunned_ConsumeTurnSkip_ReturnsFalseWhenNoStacks()
        {
            var stunned = new Stunned(1);
            stunned.ConsumeTurnSkip(); // consume the only stack

            var result = stunned.ConsumeTurnSkip();

            Assert.IsFalse(result, "ConsumeTurnSkip should return false when no stacks remain");
        }

        // ---- Duration mirrors stacks ----

        [Test]
        public void Stunned_DurationEqualsStacks()
        {
            var stunned = new Stunned(3);

            Assert.AreEqual(3, stunned.Duration, "Duration should equal the number of stacks");
            Assert.AreEqual(3, stunned.Stacks);
        }

        [Test]
        public void Stunned_DurationReachesZeroAfterAllStacksConsumed()
        {
            var stunned = new Stunned(2);

            stunned.ConsumeTurnSkip();
            Assert.AreEqual(1, stunned.Duration);

            stunned.ConsumeTurnSkip();
            Assert.AreEqual(0, stunned.Duration, "Duration should reach 0 after all stacks are consumed");
        }

        // ---- No tick damage ----

        [Test]
        public void Stunned_DoesNotDealTickDamage()
        {
            var unit = CreateUnit("Test", 100, 0, 5);
            unit.ApplyStatus(new Stunned(1));

            unit.TickStatusesTurnStart();

            Assert.AreEqual(100, unit.Stats.CurrentHP, "Stunned should not deal any tick damage");
        }

        // ---- Stacking ----

        [Test]
        public void Stunned_StacksAccumulateOnReapply()
        {
            var unit = CreateUnit("Test", 100, 0, 5);
            var stun1 = new Stunned(1);
            var stun2 = new Stunned(2);

            unit.ApplyStatus(stun1);
            unit.ApplyStatus(stun2);

            Assert.AreEqual(1, unit.StatusEffects.Count, "Should only have one Stunned effect");
            Assert.AreEqual(3, stun1.Stacks, "Stacks should accumulate: 1 + 2 = 3");
        }

        // ---- IStatusEffect contract ----

        [Test]
        public void Stunned_ImplementsIStatusEffect()
        {
            IStatusEffect effect = new Stunned(1);

            Assert.AreEqual("Stunned", effect.Id);
            Assert.AreEqual(0, effect.BaseDamage);
        }

        [Test]
        public void Stunned_ImplementsITurnSkipper()
        {
            var stunned = new Stunned(1);

            Assert.IsInstanceOf<ITurnSkipper>(stunned, "Stunned should implement ITurnSkipper");
        }

        // ---- Default stacks ----

        [Test]
        public void Stunned_DefaultStacksIsOne()
        {
            var stunned = new Stunned();

            Assert.AreEqual(1, stunned.Stacks, "Default stack count should be 1");
        }

        // ---- ScriptableObject definition constructor ----

        [Test]
        public void Stunned_DataDrivenConstructor_ReadsStacksFromDefinition()
        {
            var definition = UnityEngine.ScriptableObject.CreateInstance<StunnedDefinition>();
            definition.EditorInit(3);

            var stunned = new Stunned(definition);

            Assert.AreEqual(3, stunned.Stacks);
            Assert.AreEqual("Stunned", stunned.Id);
            Assert.AreEqual(0, stunned.BaseDamage);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        // ---- Combat integration ----

        [Test]
        public void Stunned_SkipsActingUnitTurnInCombat()
        {
            // Attacker goes first (speed 10 > 5), but is stunned for 1 stack.
            // Defender has enough HP to survive the delayed first hit.
            // Defender has attack so attacker takes extra damage during the skipped turn.
            var attacker = CreateUnit("Attacker", 100, 10, 10);
            var defender = CreateUnit("Defender", 100, 10, 5);

            attacker.ApplyStatus(new Stunned(1));

            var engine = new CombatEngine();
            engine.RunFight(attacker, defender);

            // Stun should have been consumed and removed
            Assert.AreEqual(0, attacker.StatusEffects.Count, "Stun should expire after its stack is consumed");
        }

        [Test]
        public void Stunned_StunnedUnitTakesExtraDamageFromDelayedTurn()
        {
            // Attacker is stunned for 1 turn, giving defender an extra free attack.
            // Expected outcome: attacker ends at 90 HP (two hits from defender instead of one).
            var attacker = CreateUnit("Attacker", 100, 10, 10);
            var defender = CreateUnit("Defender", 15, 5, 5);

            attacker.ApplyStatus(new Stunned(1));

            new CombatEngine().RunFight(attacker, defender);

            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should be dead");
            Assert.AreEqual(90, attacker.Stats.CurrentHP,
                "Stunned attacker should have taken an extra hit due to skipped turn");
        }

        [Test]
        public void Stunned_MultipleStacks_SkipsMultipleTurns()
        {
            // Attacker has 2 stacks of stun: skips rounds 1 and 3 (its turns).
            // Defender: 25 HP, 5 attack, speed 5.
            // Without stun A would deal 10/turn and kill B in 3 hits.
            // With 2 stacks of stun, A skips 2 turns → B gets 2 extra attacks.
            var attacker = CreateUnit("Attacker", 100, 10, 10);
            var defender = CreateUnit("Defender", 25, 5, 5);

            attacker.ApplyStatus(new Stunned(2));

            new CombatEngine().RunFight(attacker, defender);

            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Defender should eventually die");
            Assert.AreEqual(0, attacker.StatusEffects.Count, "Both stun stacks should be consumed");
        }

        [Test]
        public void Stunned_ExpiredBeforeCombatEnds_UnitAttacksNormally()
        {
            // Stunned for 1 stack; after consuming it the unit should attack normally.
            // Attacker: strong enough to kill defender in one hit after stun expires.
            var attacker = CreateUnit("Attacker", 100, 50, 10);
            var defender = CreateUnit("Defender", 10, 0, 5);

            attacker.ApplyStatus(new Stunned(1));

            new CombatEngine().RunFight(attacker, defender);

            Assert.LessOrEqual(defender.Stats.CurrentHP, 0, "Attacker should kill defender after stun expires");
            Assert.AreEqual(100, attacker.Stats.CurrentHP, "Attacker should not take damage from 0-attack defender");
        }
    }
}
