using System.Linq;

using Core;
using Core.Abilities;
using Core.Artifacts;
using Core.Artifacts.Passives;
using Core.Combat;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class SpellEchoTests
    {
        private Unit CreateUnit(string name, int hp, int attack, int speed)
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

        private static void AttachSpellEcho(Unit unit)
        {
            var echo = new SpellEcho();
            echo.OnAttach(unit);
            unit.Artifacts.Add(echo);
        }

        // ---- BASIC ECHO BEHAVIOUR ----

        [Test]
        public void SpellEcho_DoublesFireball_DamageDealtToTarget()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var targetNoEcho = CreateUnit("Target", 1000, 0, 5);
            var targetWithEcho = CreateUnit("Target", 1000, 0, 5);

            var fireball = Fireball.EditorCreate(10);

            // Simulate one cast WITHOUT SpellEcho
            var contextNoEcho = new CombatContext();
            fireball.OnCast(caster, targetNoEcho, contextNoEcho);
            // No OnAbilityTriggerEvent listener — no echo fires

            // Simulate the same cast WITH SpellEcho (original + event mirrors TriggerAbilities)
            var echo = new SpellEcho();
            echo.OnAttach(caster);
            var contextWithEcho = new CombatContext();
            contextWithEcho.RegisterListener(echo);
            fireball.OnCast(caster, targetWithEcho, contextWithEcho);
            contextWithEcho.Raise(new OnAbilityTriggerEvent(caster, targetWithEcho, fireball));

            // With echo the target should have taken exactly twice as much damage
            Assert.AreEqual(990, targetNoEcho.Stats.CurrentHP,
                "Without SpellEcho a single Fireball should deal 10 damage");
            Assert.AreEqual(980, targetWithEcho.Stats.CurrentHP,
                "With SpellEcho the same Fireball should deal 20 damage (original + echo)");
        }

        [Test]
        public void SpellEcho_CastsAbilityTwice_PerCast()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 1000, 0, 5);

            var fireball = Fireball.EditorCreate(10);
            var echo = new SpellEcho();
            echo.OnAttach(caster);

            var context = new CombatContext();
            context.RegisterListener(echo);

            // Simulate one ability cast + the event that CombatEngine raises after OnCast
            fireball.OnCast(caster, target, context);
            context.Raise(new OnAbilityTriggerEvent(caster, target, fireball));

            var fireballCount = context.Actions.OfType<FireballAction>().Count();
            Assert.AreEqual(2, fireballCount,
                "SpellEcho should produce exactly 2 FireballActions per cast (original + echo)");
        }

        [Test]
        public void SpellEcho_DoesNotEcho_WhenTargetIsDead()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 5, 0, 5); // Low HP: dies from the first Fireball (10 dmg)

            caster.Abilities.Add(Fireball.EditorCreate(10));
            AttachSpellEcho(caster);

            var engine = new CombatEngine();
            var actions = engine.RunFight(caster, target);

            // Target dies from the first cast; echo should be skipped
            var fireballActions = actions.OfType<FireballAction>().Where(a => a.Source == caster).ToList();
            Assert.AreEqual(1, fireballActions.Count,
                "Echo should not fire when the target is dead after the first cast");
        }

        [Test]
        public void SpellEcho_DoesNotEchoItselfRecursively()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 1000, 0, 5);

            caster.Abilities.Add(Fireball.EditorCreate(1));
            AttachSpellEcho(caster);

            // If re-entrance guard is missing this would cause infinite recursion / stack overflow
            Assert.DoesNotThrow(() =>
            {
                var engine = new CombatEngine();
                engine.RunFight(caster, target);
            }, "SpellEcho must not recurse infinitely");
        }

        [Test]
        public void SpellEcho_WorksWithArcaneMissiles()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 1000, 0, 5);

            // ArcaneMissiles defaults: 3 missiles × 5 damage = 15 damage per cast
            var missiles = ArcaneMissiles.EditorCreate();
            var echo = new SpellEcho();
            echo.OnAttach(caster);

            var context = new CombatContext();
            context.RegisterListener(echo);

            // Simulate one ability cast + event
            missiles.OnCast(caster, target, context);
            context.Raise(new OnAbilityTriggerEvent(caster, target, missiles));

            // Original 3 missiles + 3 echoed missiles = 6 total
            var missileCount = context.Actions.OfType<ArcaneMissilesAction>().Count();
            Assert.AreEqual(6, missileCount,
                "SpellEcho should fire 6 ArcaneMissile hits (3 original + 3 echo)");
        }

        [Test]
        public void SpellEcho_OnlyAffectsOwner_NotOpponent()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var opponent = CreateUnit("Opponent", 1000, 0, 5);

            caster.Abilities.Add(Fireball.EditorCreate(5));
            opponent.Abilities.Add(Fireball.EditorCreate(5));

            // Only caster gets SpellEcho
            AttachSpellEcho(caster);

            var engine = new CombatEngine();
            var actions = engine.RunFight(caster, opponent);

            var casterFireballs = actions.OfType<FireballAction>().Count(a => a.Source == caster);
            var opponentFireballs = actions.OfType<FireballAction>().Count(a => a.Source == opponent);

            // After the same number of turns, caster should have more Fireball hits
            Assert.Greater(casterFireballs, opponentFireballs,
                "SpellEcho should only benefit the owning unit");
        }

        // ---- ARTIFACT APPLIER INTEGRATION ----

        [Test]
        public void ArtifactApplier_SpellEcho_AddsSpellEchoToUnit()
        {
            var unit = CreateUnit("Hero", 100, 10, 5);
            var artifactDef = UnityEngine.ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifactDef.EditorInit(ArtifactId.SpellEcho, "Spell Echo", "Your spells are cast twice.",
                Core.Rarity.Epic, ArtifactTag.None, ArtifactEffectType.AddArtifact, true);

            ArtifactApplier.ApplyToPlayer(artifactDef, unit);

            Assert.AreEqual(1, unit.Artifacts.Count);
            Assert.IsInstanceOf<SpellEcho>(unit.Artifacts[0]);
        }
    }
}
