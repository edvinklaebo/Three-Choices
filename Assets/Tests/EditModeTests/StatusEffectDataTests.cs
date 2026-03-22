using Core;
using Core.Artifacts;
using Core.Artifacts.Passives;
using Core.Passives;
using Core.Passives.Definitions;
using Core.StatusEffects;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    /// <summary>
    ///     Tests for the data-driven status effect system:
    ///     <see cref="BleedDefinition"/>, <see cref="PoisonDefinition"/>, <see cref="BurnDefinition"/> ScriptableObjects
    ///     and the runtime instances seeded from them.
    ///     Verifies that all balance values come from the SO and that the SO is never mutated at runtime.
    /// </summary>
    public class StatusEffectDataTests
    {
        private static Unit CreateUnit(string name, int hp, int armor = 0)
            => new Unit(name) { Stats = new Stats { MaxHP = hp, CurrentHP = hp, Armor = armor } };

        // ---- BleedDefinition ----

        [Test]
        public void BleedDefinition_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 5, duration: 4, baseDamage: 3);

            var bleed = new Bleed(data);

            Assert.AreEqual(5, bleed.Stacks);
            Assert.AreEqual(4, bleed.Duration);
            Assert.AreEqual(3, bleed.BaseDamage);
        }

        [Test]
        public void BleedDefinition_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 3, duration: 2, baseDamage: 1);

            var unit = CreateUnit("Target", 100);
            var bleed = new Bleed(data);
            unit.ApplyStatus(bleed);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            // The SO should retain the original values despite ticks
            Assert.AreEqual(3, data.Stacks, "BleedDefinition.Stacks must not be mutated at runtime");
            Assert.AreEqual(2, data.Duration, "BleedDefinition.Duration must not be mutated at runtime");
            Assert.AreEqual(1, data.BaseDamage, "BleedDefinition.BaseDamage must not be mutated at runtime");
        }

        // ---- PoisonDefinition ----

        [Test]
        public void PoisonDefinition_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<PoisonDefinition>();
            data.EditorInit(stacks: 4, duration: 6, baseDamage: 2);

            var poison = new Poison(data);

            Assert.AreEqual(4, poison.Stacks);
            Assert.AreEqual(6, poison.Duration);
            Assert.AreEqual(2, poison.BaseDamage);
        }

        [Test]
        public void PoisonDefinition_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<PoisonDefinition>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 1);

            var unit = CreateUnit("Target", 100);
            var poison = new Poison(data);
            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(2, data.Stacks, "PoisonDefinition.Stacks must not be mutated at runtime");
            Assert.AreEqual(3, data.Duration, "PoisonDefinition.Duration must not be mutated at runtime");
            Assert.AreEqual(1, data.BaseDamage, "PoisonDefinition.BaseDamage must not be mutated at runtime");
        }

        // ---- BurnDefinition ----

        [Test]
        public void BurnDefinition_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<BurnDefinition>();
            data.EditorInit(duration: 5, baseDamage: 8);

            var burn = new Burn(data);

            Assert.AreEqual(5, burn.Duration);
            Assert.AreEqual(8, burn.BaseDamage);
        }

        [Test]
        public void BurnDefinition_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<BurnDefinition>();
            data.EditorInit(duration: 3, baseDamage: 6);

            var unit = CreateUnit("Target", 100);
            var burn = new Burn(data);
            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(3, data.Duration, "BurnDefinition.Duration must not be mutated at runtime");
            Assert.AreEqual(6, data.BaseDamage, "BurnDefinition.BaseDamage must not be mutated at runtime");
        }

        [Test]
        public void BurnDefinition_RefreshesDuration_OnReapply()
        {
            var data = ScriptableObject.CreateInstance<BurnDefinition>();
            data.EditorInit(duration: 4, baseDamage: 5);

            var unit = CreateUnit("Target", 100);
            var burn = new Burn(data);
            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart(); // duration → 3

            // Reapply: duration should reset to the SO value (4)
            unit.ApplyStatus(new Burn(data));
            Assert.AreEqual(4, burn.Duration, "Duration should reset to data.Duration on re-apply");
        }

        // ---- BleedUpgrade with BleedDefinition ----

        [Test]
        public void BleedUpgrade_DataCtor_AppliesCorrectStacksOnHit()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 3, duration: 4, baseDamage: 1);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new BleedUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(3, bleed.Stacks, "Stacks should match BleedDefinition");
            Assert.AreEqual(4, bleed.Duration, "Duration should match BleedDefinition");
        }

        [Test]
        public void BleedDefinition_DoesNotMutate_WhenBleedUpgradeApplies()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new BleedUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(2, data.Stacks, "BleedDefinition.Stacks must not change after multiple hits");
        }

        // ---- PoisonUpgrade with PoisonDefinition ----

        [Test]
        public void PoisonUpgrade_DataCtor_AppliesCorrectStacksOnHit()
        {
            var data = ScriptableObject.CreateInstance<PoisonDefinition>();
            data.EditorInit(stacks: 5, duration: 6, baseDamage: 3);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new PoisonUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var poison = defender.StatusEffects[0];
            Assert.AreEqual("Poison", poison.Id);
            Assert.AreEqual(5, poison.Stacks, "Stacks should match PoisonDefinition");
            Assert.AreEqual(6, poison.Duration, "Duration should match PoisonDefinition");
            Assert.AreEqual(3, poison.BaseDamage, "BaseDamage should match PoisonDefinition");
        }

        [Test]
        public void PoisonDefinition_DoesNotMutate_WhenPoisonUpgradeApplies()
        {
            var data = ScriptableObject.CreateInstance<PoisonDefinition>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new PoisonUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(2, data.Stacks, "PoisonDefinition.Stacks must not change after multiple hits");
        }

        // ---- BlazingTorch with BurnDefinition ----

        [Test]
        public void BlazingTorch_DataCtor_AppliesCorrectBurnOnHit()
        {
            var data = ScriptableObject.CreateInstance<BurnDefinition>();
            data.EditorInit(duration: 2, baseDamage: 7);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var torch = new BlazingTorch(data);
            torch.OnAttach(attacker);

            attacker.RaiseOnHit(defender, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var burn = defender.StatusEffects[0] as Burn;
            Assert.IsNotNull(burn);
            Assert.AreEqual(7, burn.BaseDamage, "BaseDamage should match BurnDefinition");
            Assert.AreEqual(2, burn.Duration, "Duration should match BurnDefinition");
        }

        // ---- BloodRitual with BleedDefinition ----

        [Test]
        public void BloodRitual_DataCtor_AppliesCorrectBleedOnHit()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 4, duration: 5, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var ritual = new BloodRitual(data);
            ritual.OnAttach(attacker);

            attacker.RaiseOnHit(defender, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(4, bleed.Stacks, "Stacks should match BleedDefinition");
            Assert.AreEqual(5, bleed.Duration, "Duration should match BleedDefinition");
        }

        // ---- PoisonAmplifier with PoisonDefinition ----

        [Test]
        public void PoisonAmplifier_DataCtor_AppliesCorrectBonusStacks()
        {
            var poisonData = ScriptableObject.CreateInstance<PoisonDefinition>();
            poisonData.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var amplifierData = ScriptableObject.CreateInstance<PoisonDefinition>();
            amplifierData.EditorInit(stacks: 3, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var poisonUpgrade = new PoisonUpgrade(attacker, poisonData);
            poisonUpgrade.OnAttach(attacker);
            attacker.Passives.Add(poisonUpgrade);

            var amplifier = new PoisonAmplifier(amplifierData);
            amplifier.OnAttach(attacker);
            attacker.Artifacts.Add(amplifier);

            attacker.RaiseOnHit(defender, 10);

            // PoisonUpgrade adds 2 stacks, amplifier adds 3 bonus stacks → total 5
            var poison = defender.StatusEffects[0] as Poison;
            Assert.IsNotNull(poison);
            Assert.AreEqual(5, poison.Stacks, "Stacks should be PoisonUpgrade stacks + amplifier bonus stacks");
        }

        // ---- PassiveDefinition concrete classes with data SOs ----

        [Test]
        public void BleedPassiveDefinition_UsesBleedDefinition_WhenSet()
        {
            var data = ScriptableObject.CreateInstance<BleedDefinition>();
            data.EditorInit(stacks: 7, duration: 5, baseDamage: 3);

            var definition = ScriptableObject.CreateInstance<BleedPassiveDefinition>();
            definition.EditorInit("bleed", "Bleed Upgrade", bleedDefinition: data);

            var unit = CreateUnit("Hero", 100);
            definition.Apply(unit);

            var attacker = unit;
            var defender = CreateUnit("Enemy", 100);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual(7, bleed.Stacks, "Bleed stacks should come from BleedDefinition asset");
            Assert.AreEqual(5, bleed.Duration, "Bleed duration should come from BleedDefinition asset");
        }

        [Test]
        public void PoisonPassiveDefinition_UsesPoisonDefinition_WhenSet()
        {
            var data = ScriptableObject.CreateInstance<PoisonDefinition>();
            data.EditorInit(stacks: 6, duration: 4, baseDamage: 2);

            var definition = ScriptableObject.CreateInstance<PoisonPassiveDefinition>();
            definition.EditorInit("poison", "Poison Upgrade", poisonDefinition: data);

            var unit = CreateUnit("Hero", 100);
            definition.Apply(unit);

            var defender = CreateUnit("Enemy", 100);
            defender.ApplyDamage(unit, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var poison = defender.StatusEffects[0];
            Assert.AreEqual(6, poison.Stacks, "Poison stacks should come from PoisonDefinition asset");
            Assert.AreEqual(4, poison.Duration, "Poison duration should come from PoisonDefinition asset");
        }

        // ---- ArtifactDefinition with data SOs ----

        [Test]
        public void ArtifactDefinition_BlazingTorch_UsesBurnDefinition_WhenSet()
        {
            var burnData = ScriptableObject.CreateInstance<BurnDefinition>();
            burnData.EditorInit(duration: 2, baseDamage: 9);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BlazingTorch, "Blazing Torch", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(burnDefinition: burnData);

            var unit = CreateUnit("Hero", 100);
            ArtifactApplier.ApplyToPlayer(artifact, unit);

            var target = CreateUnit("Enemy", 100);
            unit.RaiseOnHit(target, 10);

            Assert.AreEqual(1, target.StatusEffects.Count);
            var burn = target.StatusEffects[0] as Burn;
            Assert.IsNotNull(burn);
            Assert.AreEqual(9, burn.BaseDamage, "Burn damage should come from BurnDefinition asset");
            Assert.AreEqual(2, burn.Duration, "Burn duration should come from BurnDefinition asset");
        }

        [Test]
        public void ArtifactDefinition_BloodRitual_UsesBleedDefinition_WhenSet()
        {
            var bleedData = ScriptableObject.CreateInstance<BleedDefinition>();
            bleedData.EditorInit(stacks: 5, duration: 3, baseDamage: 2);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BloodRitual, "Blood Ritual", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(bleedDefinition: bleedData);

            var unit = CreateUnit("Hero", 100);
            ArtifactApplier.ApplyToPlayer(artifact, unit);

            var target = CreateUnit("Enemy", 100);
            unit.RaiseOnHit(target, 10);

            Assert.AreEqual(1, target.StatusEffects.Count);
            var bleed = target.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(5, bleed.Stacks, "Bleed stacks should come from BleedDefinition asset");
        }

        [Test]
        public void ArtifactDefinition_PoisonDarts_UsesPoisonDefinition_WhenSet()
        {
            var poisonData = ScriptableObject.CreateInstance<PoisonDefinition>();
            poisonData.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var amplifierData = ScriptableObject.CreateInstance<PoisonDefinition>();
            amplifierData.EditorInit(stacks: 4, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Hero", 100);

            // Attach a PoisonUpgrade so target already has Poison when amplifier fires
            var poisonUpgrade = new PoisonUpgrade(attacker, poisonData);
            poisonUpgrade.OnAttach(attacker);
            attacker.Passives.Add(poisonUpgrade);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.PoisonDarts, "Poison Darts", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(poisonDefinition: amplifierData);

            ArtifactApplier.ApplyToPlayer(artifact, attacker);

            var target = CreateUnit("Enemy", 100);
            attacker.RaiseOnHit(target, 10);

            // PoisonUpgrade: 2 stacks; PoisonAmplifier: 4 bonus stacks → total 6
            var poison = target.StatusEffects[0] as Poison;
            Assert.IsNotNull(poison);
            Assert.AreEqual(6, poison.Stacks, "Stacks should be upgrade + amplifier from PoisonDefinition asset");
        }

        // ---- Backward-compat: fallback to code defaults when no data SO is set ----

        [Test]
        public void ArtifactDefinition_BlazingTorch_FallsBackToDefaults_WhenNoBurnDefinition()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BlazingTorch, "Blazing Torch", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            // No EditorInitStatusEffectData call → BurnDefinition remains null

            var unit = CreateUnit("Hero", 100);
            ArtifactApplier.ApplyToPlayer(artifact, unit);

            var target = CreateUnit("Enemy", 100);
            unit.RaiseOnHit(target, 10);

            // Should apply Burn with hardcoded defaults (burnDuration=3, burnDamage=4)
            Assert.AreEqual(1, target.StatusEffects.Count, "Burn should still be applied with defaults");
            var burn = target.StatusEffects[0] as Burn;
            Assert.IsNotNull(burn);
            Assert.AreEqual(4, burn.BaseDamage, "Should use hardcoded default damage of 4");
            Assert.AreEqual(3, burn.Duration, "Should use hardcoded default duration of 3");
        }
    }
}
