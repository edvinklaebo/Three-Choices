using Core;
using Core.Artifacts;
using Core.Artifacts.Passives;
using Core.Passives;
using Core.StatusEffects;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    /// <summary>
    ///     Tests for the data-driven status effect system:
    ///     <see cref="BleedData"/>, <see cref="PoisonData"/>, <see cref="BurnData"/> ScriptableObjects
    ///     and the runtime instances seeded from them.
    ///     Verifies that all balance values come from the SO and that the SO is never mutated at runtime.
    /// </summary>
    public class StatusEffectDataTests
    {
        private static Unit CreateUnit(string name, int hp, int armor = 0)
            => new Unit(name) { Stats = new Stats { MaxHP = hp, CurrentHP = hp, Armor = armor } };

        // ---- BleedData ----

        [Test]
        public void BleedData_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 5, duration: 4, baseDamage: 3);

            var bleed = new Bleed(data);

            Assert.AreEqual(5, bleed.Stacks);
            Assert.AreEqual(4, bleed.Duration);
            Assert.AreEqual(3, bleed.BaseDamage);
        }

        [Test]
        public void BleedData_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 3, duration: 2, baseDamage: 1);

            var unit = CreateUnit("Target", 100);
            var bleed = new Bleed(data);
            unit.ApplyStatus(bleed);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            // The SO should retain the original values despite ticks
            Assert.AreEqual(3, data.Stacks, "BleedData.Stacks must not be mutated at runtime");
            Assert.AreEqual(2, data.Duration, "BleedData.Duration must not be mutated at runtime");
            Assert.AreEqual(1, data.BaseDamage, "BleedData.BaseDamage must not be mutated at runtime");
        }

        // ---- PoisonData ----

        [Test]
        public void PoisonData_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<PoisonData>();
            data.EditorInit(stacks: 4, duration: 6, baseDamage: 2);

            var poison = new Poison(data);

            Assert.AreEqual(4, poison.Stacks);
            Assert.AreEqual(6, poison.Duration);
            Assert.AreEqual(2, poison.BaseDamage);
        }

        [Test]
        public void PoisonData_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<PoisonData>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 1);

            var unit = CreateUnit("Target", 100);
            var poison = new Poison(data);
            unit.ApplyStatus(poison);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(2, data.Stacks, "PoisonData.Stacks must not be mutated at runtime");
            Assert.AreEqual(3, data.Duration, "PoisonData.Duration must not be mutated at runtime");
            Assert.AreEqual(1, data.BaseDamage, "PoisonData.BaseDamage must not be mutated at runtime");
        }

        // ---- BurnData ----

        [Test]
        public void BurnData_Ctor_SeedsFieldsFromDefinition()
        {
            var data = ScriptableObject.CreateInstance<BurnData>();
            data.EditorInit(duration: 5, baseDamage: 8);

            var burn = new Burn(data);

            Assert.AreEqual(5, burn.Duration);
            Assert.AreEqual(8, burn.BaseDamage);
        }

        [Test]
        public void BurnData_DoesNotMutateSOAtRuntime()
        {
            var data = ScriptableObject.CreateInstance<BurnData>();
            data.EditorInit(duration: 3, baseDamage: 6);

            var unit = CreateUnit("Target", 100);
            var burn = new Burn(data);
            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart();
            unit.TickStatusesTurnStart();

            Assert.AreEqual(3, data.Duration, "BurnData.Duration must not be mutated at runtime");
            Assert.AreEqual(6, data.BaseDamage, "BurnData.BaseDamage must not be mutated at runtime");
        }

        [Test]
        public void BurnData_RefreshesDuration_OnReapply()
        {
            var data = ScriptableObject.CreateInstance<BurnData>();
            data.EditorInit(duration: 4, baseDamage: 5);

            var unit = CreateUnit("Target", 100);
            var burn = new Burn(data);
            unit.ApplyStatus(burn);
            unit.TickStatusesTurnStart(); // duration → 3

            // Reapply: duration should reset to the SO value (4)
            unit.ApplyStatus(new Burn(data));
            Assert.AreEqual(4, burn.Duration, "Duration should reset to data.Duration on re-apply");
        }

        // ---- BleedUpgrade with BleedData ----

        [Test]
        public void BleedUpgrade_DataCtor_AppliesCorrectStacksOnHit()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 3, duration: 4, baseDamage: 1);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new BleedUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(3, bleed.Stacks, "Stacks should match BleedData");
            Assert.AreEqual(4, bleed.Duration, "Duration should match BleedData");
        }

        [Test]
        public void BleedData_DoesNotMutate_WhenBleedUpgradeApplies()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new BleedUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(2, data.Stacks, "BleedData.Stacks must not change after multiple hits");
        }

        // ---- PoisonUpgrade with PoisonData ----

        [Test]
        public void PoisonUpgrade_DataCtor_AppliesCorrectStacksOnHit()
        {
            var data = ScriptableObject.CreateInstance<PoisonData>();
            data.EditorInit(stacks: 5, duration: 6, baseDamage: 3);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new PoisonUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var poison = defender.StatusEffects[0];
            Assert.AreEqual("Poison", poison.Id);
            Assert.AreEqual(5, poison.Stacks, "Stacks should match PoisonData");
            Assert.AreEqual(6, poison.Duration, "Duration should match PoisonData");
            Assert.AreEqual(3, poison.BaseDamage, "BaseDamage should match PoisonData");
        }

        [Test]
        public void PoisonData_DoesNotMutate_WhenPoisonUpgradeApplies()
        {
            var data = ScriptableObject.CreateInstance<PoisonData>();
            data.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var upgrade = new PoisonUpgrade(attacker, data);
            upgrade.OnAttach(attacker);

            defender.ApplyDamage(attacker, 10);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(2, data.Stacks, "PoisonData.Stacks must not change after multiple hits");
        }

        // ---- BlazingTorch with BurnData ----

        [Test]
        public void BlazingTorch_DataCtor_AppliesCorrectBurnOnHit()
        {
            var data = ScriptableObject.CreateInstance<BurnData>();
            data.EditorInit(duration: 2, baseDamage: 7);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var torch = new BlazingTorch(data);
            torch.OnAttach(attacker);

            attacker.RaiseOnHit(defender, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var burn = defender.StatusEffects[0] as Burn;
            Assert.IsNotNull(burn);
            Assert.AreEqual(7, burn.BaseDamage, "BaseDamage should match BurnData");
            Assert.AreEqual(2, burn.Duration, "Duration should match BurnData");
        }

        // ---- BloodRitual with BleedData ----

        [Test]
        public void BloodRitual_DataCtor_AppliesCorrectBleedOnHit()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 4, duration: 5, baseDamage: 2);

            var attacker = CreateUnit("Attacker", 100);
            var defender = CreateUnit("Defender", 100);

            var ritual = new BloodRitual(data);
            ritual.OnAttach(attacker);

            attacker.RaiseOnHit(defender, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(4, bleed.Stacks, "Stacks should match BleedData");
            Assert.AreEqual(5, bleed.Duration, "Duration should match BleedData");
        }

        // ---- PoisonAmplifier with PoisonData ----

        [Test]
        public void PoisonAmplifier_DataCtor_AppliesCorrectBonusStacks()
        {
            var poisonData = ScriptableObject.CreateInstance<PoisonData>();
            poisonData.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var amplifierData = ScriptableObject.CreateInstance<PoisonData>();
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

        // ---- PassiveDefinition with data SOs ----

        [Test]
        public void PassiveDefinition_BleedPassive_UsesBleedData_WhenSet()
        {
            var data = ScriptableObject.CreateInstance<BleedData>();
            data.EditorInit(stacks: 7, duration: 5, baseDamage: 3);

            var definition = ScriptableObject.CreateInstance<PassiveDefinition>();
            definition.EditorInit("bleed", "Bleed Upgrade", PassiveId.Bleed, bleedData: data, poisonData: null);

            var unit = CreateUnit("Hero", 100);
            definition.Apply(unit);

            var attacker = unit;
            var defender = CreateUnit("Enemy", 100);
            defender.ApplyDamage(attacker, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var bleed = defender.StatusEffects[0];
            Assert.AreEqual(7, bleed.Stacks, "Bleed stacks should come from BleedData asset");
            Assert.AreEqual(5, bleed.Duration, "Bleed duration should come from BleedData asset");
        }

        [Test]
        public void PassiveDefinition_PoisonPassive_UsesPoisonData_WhenSet()
        {
            var data = ScriptableObject.CreateInstance<PoisonData>();
            data.EditorInit(stacks: 6, duration: 4, baseDamage: 2);

            var definition = ScriptableObject.CreateInstance<PassiveDefinition>();
            definition.EditorInit("poison", "Poison Upgrade", PassiveId.Poison, bleedData: null, poisonData: data);

            var unit = CreateUnit("Hero", 100);
            definition.Apply(unit);

            var defender = CreateUnit("Enemy", 100);
            defender.ApplyDamage(unit, 10);

            Assert.AreEqual(1, defender.StatusEffects.Count);
            var poison = defender.StatusEffects[0];
            Assert.AreEqual(6, poison.Stacks, "Poison stacks should come from PoisonData asset");
            Assert.AreEqual(4, poison.Duration, "Poison duration should come from PoisonData asset");
        }

        // ---- ArtifactDefinition with data SOs ----

        [Test]
        public void ArtifactDefinition_BlazingTorch_UsesBurnData_WhenSet()
        {
            var burnData = ScriptableObject.CreateInstance<BurnData>();
            burnData.EditorInit(duration: 2, baseDamage: 9);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BlazingTorch, "Blazing Torch", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(burnData: burnData);

            var unit = CreateUnit("Hero", 100);
            ArtifactApplier.ApplyToPlayer(artifact, unit);

            var target = CreateUnit("Enemy", 100);
            unit.RaiseOnHit(target, 10);

            Assert.AreEqual(1, target.StatusEffects.Count);
            var burn = target.StatusEffects[0] as Burn;
            Assert.IsNotNull(burn);
            Assert.AreEqual(9, burn.BaseDamage, "Burn damage should come from BurnData asset");
            Assert.AreEqual(2, burn.Duration, "Burn duration should come from BurnData asset");
        }

        [Test]
        public void ArtifactDefinition_BloodRitual_UsesBleedData_WhenSet()
        {
            var bleedData = ScriptableObject.CreateInstance<BleedData>();
            bleedData.EditorInit(stacks: 5, duration: 3, baseDamage: 2);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BloodRitual, "Blood Ritual", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(bleedData: bleedData);

            var unit = CreateUnit("Hero", 100);
            ArtifactApplier.ApplyToPlayer(artifact, unit);

            var target = CreateUnit("Enemy", 100);
            unit.RaiseOnHit(target, 10);

            Assert.AreEqual(1, target.StatusEffects.Count);
            var bleed = target.StatusEffects[0];
            Assert.AreEqual("Bleed", bleed.Id);
            Assert.AreEqual(5, bleed.Stacks, "Bleed stacks should come from BleedData asset");
        }

        [Test]
        public void ArtifactDefinition_PoisonDarts_UsesPoisonData_WhenSet()
        {
            var poisonData = ScriptableObject.CreateInstance<PoisonData>();
            poisonData.EditorInit(stacks: 2, duration: 3, baseDamage: 2);

            var amplifierData = ScriptableObject.CreateInstance<PoisonData>();
            amplifierData.EditorInit(stacks: 4, duration: 3, baseDamage: 2);

            var attacker = CreateUnit("Hero", 100);

            // Attach a PoisonUpgrade so target already has Poison when amplifier fires
            var poisonUpgrade = new PoisonUpgrade(attacker, poisonData);
            poisonUpgrade.OnAttach(attacker);
            attacker.Passives.Add(poisonUpgrade);

            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.PoisonDarts, "Poison Darts", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            artifact.EditorInitStatusEffectData(poisonData: amplifierData);

            ArtifactApplier.ApplyToPlayer(artifact, attacker);

            var target = CreateUnit("Enemy", 100);
            attacker.RaiseOnHit(target, 10);

            // PoisonUpgrade: 2 stacks; PoisonAmplifier: 4 bonus stacks → total 6
            var poison = target.StatusEffects[0] as Poison;
            Assert.IsNotNull(poison);
            Assert.AreEqual(6, poison.Stacks, "Stacks should be upgrade + amplifier from PoisonData asset");
        }

        // ---- Backward-compat: fallback to code defaults when no data SO is set ----

        [Test]
        public void ArtifactDefinition_BlazingTorch_FallsBackToDefaults_WhenNoBurnData()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit(ArtifactId.BlazingTorch, "Blazing Torch", "desc",
                                Rarity.Common, ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            // No EditorInitStatusEffectData call → BurnData remains null

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
