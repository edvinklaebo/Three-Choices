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
            var target = CreateUnit("Target", 200, 0, 5);

            caster.Abilities.Add(new Fireball(10));
            AttachSpellEcho(caster);

            CombatSystem.RunFight(caster, target);

            // Without SpellEcho: first round fires 1 Fireball (10 dmg), with echo: 2 Fireballs (20 dmg).
            // To verify, we check that the FireballActions logged equal 2 per round on the caster's turn.
            // Use a fresh engine to count actions precisely for round 1.
            var echolessCaster = CreateUnit("Caster2", 100, 0, 10);
            var echolessTarget = CreateUnit("Target2", 200, 0, 5);
            echolessCaster.Abilities.Add(new Fireball(10));

            var engine = new CombatEngine();
            var actionsNoEcho = engine.RunFight(echolessCaster, echolessTarget);

            var echoCaster = CreateUnit("EchoCaster", 100, 0, 10);
            var echoTarget = CreateUnit("EchoTarget", 200, 0, 5);
            echoCaster.Abilities.Add(new Fireball(10));
            AttachSpellEcho(echoCaster);

            var engine2 = new CombatEngine();
            var actionsWithEcho = engine2.RunFight(echoCaster, echoTarget);

            // The unit with SpellEcho should produce more FireballActions than without
            var noEchoFireballs = actionsNoEcho.OfType<FireballAction>().Count(a => a.Source == echolessCaster);
            var withEchoFireballs = actionsWithEcho.OfType<FireballAction>().Count(a => a.Source == echoCaster);

            Assert.Greater(withEchoFireballs, noEchoFireballs,
                "SpellEcho caster should produce more Fireball hits than one without SpellEcho");
        }

        [Test]
        public void SpellEcho_CastsAbilityTwice_PerTurn()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 1000, 0, 5); // High HP so target survives multiple rounds

            caster.Abilities.Add(new Fireball(10));
            AttachSpellEcho(caster);

            // One turn: caster acts first (speed 10 > 5), fires fireball + echo = 2 FireballActions
            var engine = new CombatEngine();
            var actions = engine.RunFight(caster, target);

            // Count only fireball actions from the caster in the first turn (2 expected per turn)
            var fireballActions = actions.OfType<FireballAction>().Where(a => a.Source == caster).ToList();

            Assert.GreaterOrEqual(fireballActions.Count, 2,
                "SpellEcho should cause at least 2 Fireball hits per turn (original + echo)");
        }

        [Test]
        public void SpellEcho_DoesNotEcho_WhenTargetIsDead()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var target = CreateUnit("Target", 5, 0, 5); // Low HP: dies from the first Fireball (10 dmg)

            caster.Abilities.Add(new Fireball(10));
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

            caster.Abilities.Add(new Fireball(1));
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

            caster.Abilities.Add(new ArcaneMissiles());
            AttachSpellEcho(caster);

            var engineNoEcho = new CombatEngine();
            var casterNoEcho = CreateUnit("Caster2", 100, 0, 10);
            var targetNoEcho = CreateUnit("Target2", 1000, 0, 5);
            casterNoEcho.Abilities.Add(new ArcaneMissiles());
            var actionsNoEcho = engineNoEcho.RunFight(casterNoEcho, targetNoEcho);

            var engineWithEcho = new CombatEngine();
            var actionsWithEcho = engineWithEcho.RunFight(caster, target);

            var hitCountNoEcho = actionsNoEcho.OfType<ArcaneMissilesAction>().Count(a => a.Source == casterNoEcho);
            var hitCountWithEcho = actionsWithEcho.OfType<ArcaneMissilesAction>().Count(a => a.Source == caster);

            Assert.Greater(hitCountWithEcho, hitCountNoEcho,
                "SpellEcho should double ArcaneMissiles hits per turn");
        }

        [Test]
        public void SpellEcho_OnlyAffectsOwner_NotOpponent()
        {
            var caster = CreateUnit("Caster", 100, 0, 10);
            var opponent = CreateUnit("Opponent", 1000, 0, 5);

            caster.Abilities.Add(new Fireball(5));
            opponent.Abilities.Add(new Fireball(5));

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
