using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for the CombatPhase system and DamagePhaseEvent flow.
    /// Verifies that phases fire in order, listeners can mutate DamageContext,
    /// and HP is only mutated through ResolveAttack.
    /// </summary>
    public class CombatPhaseTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int speed = 10)
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

        [Test]
        public void ResolveAttack_AppliesDamage_ToTarget()
        {
            var source = CreateUnit("Attacker", 100, 20);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            context.DealDamage(source, target, 20);

            Assert.AreEqual(80, target.Stats.CurrentHP, "Target should take 20 damage");
        }

        [Test]
        public void ResolveAttack_CreatesDamageAction()
        {
            var source = CreateUnit("Attacker", 100, 20);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            context.DealDamage(source, target, 20);

            var damageActions = context.Actions.OfType<DamageAction>().ToList();
            Assert.AreEqual(1, damageActions.Count, "Should create exactly one DamageAction");
            Assert.AreEqual(20, damageActions[0].Amount, "DamageAction should record 20 damage");
        }

        [Test]
        public void ResolveAttack_CreatesDeathAction_WhenTargetDies()
        {
            var source = CreateUnit("Attacker", 100, 100);
            var target = CreateUnit("Defender", 50, 0);
            var context = new CombatContext();

            context.DealDamage(source, target, 100);

            Assert.IsTrue(target.IsDead, "Target should be dead");
            var deathActions = context.Actions.OfType<DeathAction>().ToList();
            Assert.AreEqual(1, deathActions.Count, "Should create exactly one DeathAction");
        }

        [Test]
        public void DamagePhaseEvent_FiredForEachPhase()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            var phasesObserved = new List<CombatPhase>();
            context.On<DamagePhaseEvent>(evt => phasesObserved.Add(evt.Phase));

            context.DealDamage(source, target, 10);

            // DamageApplication is intentionally not raised as an event — HP is mutated inline
            // inside ResolveAttack to guarantee a single, controlled mutation point.
            CollectionAssert.Contains(phasesObserved, CombatPhase.PreAction);
            CollectionAssert.Contains(phasesObserved, CombatPhase.DamageCalculation);
            CollectionAssert.Contains(phasesObserved, CombatPhase.Mitigation);
            CollectionAssert.Contains(phasesObserved, CombatPhase.Healing);
            CollectionAssert.Contains(phasesObserved, CombatPhase.ResourceGain);
            CollectionAssert.Contains(phasesObserved, CombatPhase.StatusApplication);
            CollectionAssert.Contains(phasesObserved, CombatPhase.PostResolve);
            CollectionAssert.DoesNotContain(phasesObserved, CombatPhase.DamageApplication,
                "DamageApplication is applied inline, not via ExecutePhase");
        }

        [Test]
        public void DamagePhaseEvent_PhasesFireInOrder()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            var phasesInOrder = new List<CombatPhase>();
            context.On<DamagePhaseEvent>(evt => phasesInOrder.Add(evt.Phase));

            context.DealDamage(source, target, 10);

            // DamageApplication is applied inline (not raised as an event), so it is absent from this list.
            Assert.AreEqual(CombatPhase.PreAction, phasesInOrder[0], "PreAction should fire first");
            Assert.AreEqual(CombatPhase.DamageCalculation, phasesInOrder[1]);
            Assert.AreEqual(CombatPhase.Mitigation, phasesInOrder[2]);
            Assert.AreEqual(CombatPhase.Healing, phasesInOrder[3]);
            Assert.AreEqual(CombatPhase.ResourceGain, phasesInOrder[4]);
            Assert.AreEqual(CombatPhase.StatusApplication, phasesInOrder[5]);
            Assert.AreEqual(CombatPhase.PostResolve, phasesInOrder[6]);
            Assert.AreEqual(7, phasesInOrder.Count, "Exactly 7 phase events should fire per attack");
        }

        [Test]
        public void DamagePhaseEvent_ContextCarriesSourceAndTarget()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            Unit observedSource = null;
            Unit observedTarget = null;
            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.DamageCalculation)
                {
                    observedSource = evt.Context.Source;
                    observedTarget = evt.Context.Target;
                }
            });

            context.DealDamage(source, target, 10);

            Assert.AreEqual(source, observedSource, "Context.Source should be the attacker");
            Assert.AreEqual(target, observedTarget, "Context.Target should be the defender");

            // Source and Target on the event itself mirror the context (populated via combined constructor)
            Unit eventSource = null;
            Unit eventTarget = null;
            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.DamageCalculation)
                {
                    eventSource = evt.Source;
                    eventTarget = evt.Target;
                }
            });
            context.DealDamage(source, target, 5);
            Assert.AreEqual(source, eventSource, "Event.Source should mirror Context.Source");
            Assert.AreEqual(target, eventTarget, "Event.Target should mirror Context.Target");
        }

        [Test]
        public void DamagePhaseEvent_ModifiedDamage_AffectsFinalDamage()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            // Listener doubles ModifiedDamage during Mitigation phase
            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.Mitigation)
                    evt.Context.ModifiedDamage *= 2;
            });

            context.DealDamage(source, target, 10);

            Assert.AreEqual(80, target.Stats.CurrentHP, "ModifiedDamage doubled in Mitigation should result in 20 damage");
        }

        [Test]
        public void DamagePhaseEvent_Cancelled_SkipsAllSubsequentPhases()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.PreAction)
                    evt.Context.Cancelled = true;
            });

            context.DealDamage(source, target, 10);

            Assert.AreEqual(100, target.Stats.CurrentHP, "Cancelled attack should not deal damage");
            Assert.IsEmpty(context.Actions.OfType<DamageAction>(), "No DamageAction should be created for cancelled attack");
        }

        [Test]
        public void PendingHealing_AppliedToSource_AfterPostResolve()
        {
            var source = CreateUnit("Attacker", 100, 10);
            source.Stats.CurrentHP = 80;
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            // Simulate lifesteal: heal 5 HP in PostResolve
            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.PostResolve && evt.Context.Source == source)
                    evt.Context.PendingHealing = 5;
            });

            context.DealDamage(source, target, 10);

            Assert.AreEqual(85, source.Stats.CurrentHP, "Source should be healed by PendingHealing amount");
            var healActions = context.Actions.OfType<HealAction>().ToList();
            Assert.AreEqual(1, healActions.Count, "A HealAction should be created");
            Assert.AreEqual(5, healActions[0].Amount);
        }

        [Test]
        public void PendingStatuses_AppliedToTarget_DuringStatusApplication()
        {
            var source = CreateUnit("Attacker", 100, 10);
            var target = CreateUnit("Defender", 100, 0);
            var context = new CombatContext();

            var bleed = new Bleed(2, 3, 2);

            context.On<DamagePhaseEvent>(evt =>
            {
                if (evt.Phase == CombatPhase.StatusApplication)
                    evt.Context.PendingStatuses.Add(bleed);
            });

            context.DealDamage(source, target, 10);

            Assert.AreEqual(1, target.StatusEffects.Count, "Target should have 1 status effect");
            Assert.AreEqual("Bleed", target.StatusEffects[0].Id, "Applied status should be Bleed");
        }

        [Test]
        public void Lifesteal_QueuesHealViaPhaseSystem_NotDirectly()
        {
            var attacker = CreateUnit("Vampire", 100, 20);
            attacker.Stats.CurrentHP = 80;
            var defender = CreateUnit("Victim", 100, 0);

            var lifesteal = new Lifesteal(attacker, 0.2f);
            lifesteal.OnAttach(attacker);
            attacker.Passives.Add(lifesteal);

            var actions = CombatSystem.RunFight(attacker, defender);

            var healActions = actions.OfType<HealAction>().ToList();
            Assert.IsNotEmpty(healActions, "Lifesteal should produce HealActions via the phase system");
            foreach (var h in healActions)
                Assert.Greater(h.Amount, 0, "Each heal should have a positive amount");
        }

        [Test]
        public void DoubleStrike_SecondHit_GoesThrough_ResolveAttack()
        {
            var attacker = CreateUnit("Striker", 100, 20);
            var defender = CreateUnit("Target", 100, 0);

            var doubleStrike = new DoubleStrike(1.0f, 0.75f);
            doubleStrike.OnAttach(attacker);
            attacker.Passives.Add(doubleStrike);

            // Subscribe BEFORE running fight to capture phases from both hits
            // (We can't subscribe to CombatContext after construction, so test via action count)
            var actions = CombatSystem.RunFight(attacker, defender);

            // Both hits should produce DamageActions through ResolveAttack
            var damageActions = actions.OfType<DamageAction>().ToList();
            Assert.GreaterOrEqual(damageActions.Count, 2, "Both primary and second hit should create DamageActions via ResolveAttack");
        }

        [Test]
        public void ApplyDamage_StatusEffect_ReducesTargetHP()
        {
            var source = CreateUnit("Source", 100, 0);
            var target = CreateUnit("Target", 100, 0);
            var context = new CombatContext();

            context.DealDamage(source, target, 15);

            Assert.AreEqual(85, target.Stats.CurrentHP, "ApplyDamage should reduce target HP by the amount");
        }

        [Test]
        public void ApplyDamage_CombatEngine_StatusTick_CreatesStatusEffectAction()
        {
            var attacker = CreateUnit("A", 100, 5, 10);
            var defender = CreateUnit("B", 100, 0, 5);
            defender.ApplyStatus(new Poison(10, 3, 1));

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            var statusActions = actions.OfType<StatusEffectAction>().ToList();
            Assert.IsNotEmpty(statusActions, "Engine should create StatusEffectActions via ApplyDamage for status effects");
            Assert.IsTrue(statusActions.All(a => a.EffectName == "Poison"), "StatusEffectActions should have the correct effect name");
        }

        [Test]
        public void ApplyDamage_CombatEngine_StatusTick_DeathCreatesDeathAction()
        {
            var attacker = CreateUnit("A", 100, 0, 5);
            var defender = CreateUnit("B", 5, 0, 10);
            defender.ApplyStatus(new Poison(10, 3, 1)); // 10 damage kills 5 HP unit

            var engine = new CombatEngine();
            var actions = engine.RunFight(attacker, defender);

            Assert.IsTrue(defender.IsDead, "Defender should die from poison");
            var deathActions = actions.OfType<DeathAction>().Where(a => a.Target == defender).ToList();
            Assert.AreEqual(1, deathActions.Count, "Exactly one DeathAction should be created for the defender");
        }
    }
}
