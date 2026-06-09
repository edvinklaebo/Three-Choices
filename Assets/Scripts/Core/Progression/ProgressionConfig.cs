using System;
using System.Collections.Generic;

using UnityEngine;

namespace Core.Progression
{
    /// <summary>
    /// ScriptableObject data source for metaprogression unlocks.
    /// Supports both quick example setup and full 100-unlock generation.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Progression/Progression Config")]
    public class ProgressionConfig : ScriptableObject
    {
        public const int DefaultTargetPoints = 10000;
        public const int DefaultUnlockCount = 100;
        private const float MinimumUnlockCost = 10f;
        private const float LastUnlockIndexOffset = 1f;
        private const float CostCurveExponent = 2.05f;

        [SerializeField] [Min(1)] private int _targetPoints = DefaultTargetPoints;
        [SerializeField] [Min(1)] private int _expectedUnlockCount = DefaultUnlockCount;
        [SerializeField] private List<ProgressionUnlockDefinition> _unlocks = new();

        public int TargetPoints => _targetPoints;
        public int ExpectedUnlockCount => _expectedUnlockCount;
        public IReadOnlyList<ProgressionUnlockDefinition> Unlocks => _unlocks;

        /// <summary>
        /// Returns true when unlock count and IDs look valid.
        /// </summary>
        public bool IsValid()
        {
            if (_unlocks == null || _unlocks.Count != _expectedUnlockCount)
                return false;

            var ids = new HashSet<string>();
            for (var i = 0; i < _unlocks.Count; i++)
            {
                var unlock = _unlocks[i];
                if (unlock == null || string.IsNullOrWhiteSpace(unlock.Id))
                    return false;

                if (!ids.Add(unlock.Id))
                    return false;
            }

            return true;
        }

        public void SetUnlocks(List<ProgressionUnlockDefinition> unlocks, int expectedUnlockCount, int targetPoints)
        {
            _unlocks = unlocks ?? new List<ProgressionUnlockDefinition>();
            _expectedUnlockCount = Mathf.Max(1, expectedUnlockCount);
            _targetPoints = Mathf.Max(1, targetPoints);
        }

        /// <summary>
        /// Generates a complete linear progression with curved costs.
        /// The final unlock reaches <paramref name="targetPoints"/> total progression points.
        /// </summary>
        public void GenerateCurvedProgression(int unlockCount = DefaultUnlockCount, int targetPoints = DefaultTargetPoints)
        {
            unlockCount = Mathf.Max(1, unlockCount);
            targetPoints = Mathf.Max(1, targetPoints);
            _targetPoints = targetPoints;
            _expectedUnlockCount = unlockCount;
            _unlocks = new List<ProgressionUnlockDefinition>(unlockCount);

            var previousId = string.Empty;
            var previousCost = 0;

            for (var i = 0; i < unlockCount; i++)
            {
                var unlock = new ProgressionUnlockDefinition();
                var category = ResolveCategoryByIndex(i);
                var id = $"unlock_{i + 1:000}";
                var cost = ResolveCurvedCost(i, unlockCount, targetPoints, previousCost);
                var dependencies = new List<string>();

                // Linear progression: each unlock depends on the previous one.
                if (!string.IsNullOrEmpty(previousId))
                    dependencies.Add(previousId);

                unlock.Initialize(
                    id,
                    $"{category} Unlock {i + 1}",
                    $"Unlocks {category} progression tier {i + 1}.",
                    cost,
                    category,
                    dependencies
                );

                _unlocks.Add(unlock);
                previousId = id;
                previousCost = cost;
            }
        }

        /// <summary>
        /// Example setup with 10 unlocks for quick iteration and testing.
        /// </summary>
        public void GenerateExampleUnlocks()
        {
            _targetPoints = DefaultTargetPoints;
            _expectedUnlockCount = 10;
            _unlocks = new List<ProgressionUnlockDefinition>(10);

            AddExampleUnlock("character_warrior", "Warrior", "Unlock a sturdy melee character.", 10, ProgressionUnlockCategory.Character);
            AddExampleUnlock("passive_toughness", "Toughness I", "Gain +5 max health at run start.", 30, ProgressionUnlockCategory.PassiveUpgrade, "character_warrior");
            AddExampleUnlock("ability_firebolt", "Firebolt", "Unlock Firebolt as an active ability.", 60, ProgressionUnlockCategory.ActiveAbility, "character_warrior");
            AddExampleUnlock("artifact_iron_heart", "Iron Heart", "Unlock Iron Heart artifact drops.", 100, ProgressionUnlockCategory.Artifact, "passive_toughness");
            AddExampleUnlock("character_ranger", "Ranger", "Unlock a ranged character.", 150, ProgressionUnlockCategory.Character, "ability_firebolt");
            AddExampleUnlock("passive_critical_mastery", "Critical Mastery I", "Gain +5% critical chance.", 230, ProgressionUnlockCategory.PassiveUpgrade, "passive_toughness");
            AddExampleUnlock("ability_chain_lightning", "Chain Lightning", "Unlock Chain Lightning ability.", 350, ProgressionUnlockCategory.ActiveAbility, "ability_firebolt");
            AddExampleUnlock("artifact_vampiric_fang", "Vampiric Fang", "Unlock lifesteal-focused artifact drops.", 520, ProgressionUnlockCategory.Artifact, "artifact_iron_heart");
            AddExampleUnlock("passive_toughness_ii", "Toughness II", "Gain an additional +10 max health.", 760, ProgressionUnlockCategory.PassiveUpgrade, "passive_toughness");
            AddExampleUnlock("character_battlemage", "Battlemage", "Unlock a hybrid fighter-caster.", 1100, ProgressionUnlockCategory.Character, "character_ranger");
        }

        private void AddExampleUnlock(string id, string name, string description, int cost,
                                      ProgressionUnlockCategory category, string dependencyId = null)
        {
            var dependencies = new List<string>();
            if (!string.IsNullOrEmpty(dependencyId))
                dependencies.Add(dependencyId);

            var unlock = new ProgressionUnlockDefinition();
            unlock.Initialize(id, name, description, cost, category, dependencies);
            _unlocks.Add(unlock);
        }

        private static int ResolveCurvedCost(int index, int unlockCount, int targetPoints, int previousCost)
        {
            if (unlockCount <= 1)
                return targetPoints;

            var t = index / (unlockCount - LastUnlockIndexOffset);
            // Exponent above 2 keeps early unlocks frequent while stretching late-game pacing.
            var curved = Mathf.Pow(t, CostCurveExponent);
            var cost = Mathf.RoundToInt(Mathf.Lerp(MinimumUnlockCost, targetPoints, curved));

            if (index == unlockCount - 1)
                return targetPoints;

            return Mathf.Max(previousCost + 1, cost);
        }

        private static ProgressionUnlockCategory ResolveCategoryByIndex(int index)
        {
            var categoryIndex = index % 4;
            return categoryIndex switch
            {
                0 => ProgressionUnlockCategory.Character,
                1 => ProgressionUnlockCategory.PassiveUpgrade,
                2 => ProgressionUnlockCategory.ActiveAbility,
                _ => ProgressionUnlockCategory.Artifact
            };
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Full Progression (100 Unlocks)")]
        private void EditorGenerateFull()
        {
            GenerateCurvedProgression(DefaultUnlockCount, DefaultTargetPoints);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Generate Example Progression (10 Unlocks)")]
        private void EditorGenerateExample()
        {
            GenerateExampleUnlocks();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
