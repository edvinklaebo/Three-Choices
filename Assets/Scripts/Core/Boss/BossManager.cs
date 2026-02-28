using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Determines whether a fight is a boss fight and selects an appropriate boss
///     from the <see cref="BossRegistry"/> based on fight progression.
///     Boss fights occur every <see cref="BossFightInterval"/> fights (e.g. fights 10, 20, 30…).
/// </summary>
public class BossManager
{
    public const int BossFightInterval = 10;

    private readonly BossRegistry _registry;

    public BossManager(BossRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>Returns true when <paramref name="fightIndex"/> is a boss fight.</summary>
    public bool IsBossFight(int fightIndex)
    {
        return fightIndex > 0 && fightIndex % BossFightInterval == 0;
    }

    /// <summary>
    ///     Selects a boss for the given <paramref name="fightIndex"/>.
    ///     Prefers bosses whose <see cref="BossDefinition.DifficultyRating"/> is at or below
    ///     the current tier (<c>fightIndex / BossFightInterval</c>).
    ///     Falls back to the lowest-rated boss when no candidates match.
    ///     Returns <c>null</c> when the registry is empty.
    /// </summary>
    public BossDefinition GetBoss(int fightIndex)
    {
        var bosses = _registry.Bosses;

        if (bosses == null || bosses.Count == 0)
        {
            Log.Error("[BossManager] No bosses registered in BossRegistry");
            return null;
        }

        int tier = fightIndex / BossFightInterval;
        var candidates = new List<BossDefinition>(bosses.Count);

        for (var i = 0; i < bosses.Count; i++)
        {
            if (bosses[i].DifficultyRating <= tier)
                candidates.Add(bosses[i]);
        }

        if (candidates.Count == 0)
        {
            Log.Warning($"[BossManager] No bosses with DifficultyRating <= {tier}. Falling back to lowest-rated boss.");
            candidates = FindLowestRatedBosses(bosses);
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private static List<BossDefinition> FindLowestRatedBosses(IReadOnlyList<BossDefinition> bosses)
    {
        var minRating = bosses[0].DifficultyRating;
        for (var i = 1; i < bosses.Count; i++)
        {
            if (bosses[i].DifficultyRating < minRating)
                minRating = bosses[i].DifficultyRating;
        }

        var result = new List<BossDefinition>();
        for (var i = 0; i < bosses.Count; i++)
        {
            if (bosses[i].DifficultyRating == minRating)
                result.Add(bosses[i]);
        }

        return result;
    }
}
