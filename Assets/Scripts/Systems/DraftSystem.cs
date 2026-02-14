using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class DraftSystem
{
    private readonly IRarityRoller _rarityRoller;
    private readonly IUpgradeRepository _repository;

    public DraftSystem(IUpgradeRepository repository) : this(repository, new RarityRoller())
    {
    }

    public DraftSystem(IUpgradeRepository repository, IRarityRoller rarityRoller)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _rarityRoller = rarityRoller ?? throw new ArgumentNullException(nameof(rarityRoller));

        Log.Info("DraftSystem initialized", new
        {
            repositoryType = repository.GetType().Name,
            rarityRollerType = rarityRoller.GetType().Name
        });
    }

    public List<UpgradeDefinition> GenerateDraft(int count)
    {
        Log.Info("GenerateDraft called", new { count });

        try
        {
            var allUpgrades = _repository.GetAll();
            Log.Info("Upgrade pool retrieved", new { poolSize = allUpgrades.Count });

            var result = new List<UpgradeDefinition>();

            // Roll rarity first
            var rolledRarity = _rarityRoller.RollRarity();

            for (var i = 0; i < count; i++)
            {
                // Filter available upgrades by rarity (exclude already drafted ones)
                var availableUpgrades = allUpgrades
                    .Where(u => !result.Contains(u))
                    .ToList();

                if (availableUpgrades.Count == 0)
                {
                    Log.Warning("No more upgrades available", new { round = i });
                    break;
                }

                // Try to get upgrade of rolled rarity, fallback to lower rarities if needed
                var selected = SelectUpgradeByRarity(availableUpgrades, rolledRarity);

                if (selected == null)
                {
                    Log.Warning("No upgrade found for any rarity", new { round = i });
                    break;
                }

                Log.Info("Draft selection", new
                {
                    round = i,
                    selected = selected.DisplayName,
                    rarity = selected.GetRarity(),
                    rolledRarity,
                    remainingPool = availableUpgrades.Count - 1
                });

                result.Add(selected);
            }

            Log.Info("Draft generated", new
            {
                draftSize = result.Count,
                selectedUpgrades = string.Join(",", result.ConvertAll(u => u.DisplayName))
            });

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "GenerateDraft failed", new { count });
            throw;
        }
    }

    private UpgradeDefinition SelectUpgradeByRarity(List<UpgradeDefinition> availableUpgrades, Rarity targetRarity)
    {
        // Try target rarity first
        var pool = availableUpgrades.Where(u => u.GetRarity() == targetRarity).ToList();

        if (pool.Count > 0) return pool[Random.Range(0, pool.Count)];

        // Fallback to next lower rarity
        var fallbackRarity = GetNextLowerRarity(targetRarity);
        if (fallbackRarity.HasValue)
        {
            Log.Info("Rarity fallback", new { from = targetRarity, to = fallbackRarity.Value });
            return SelectUpgradeByRarity(availableUpgrades, fallbackRarity.Value);
        }

        // If no rarity match found, return null
        return null;
    }

    private Rarity? GetNextLowerRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Epic => Rarity.Rare,
            Rarity.Rare => Rarity.Uncommon,
            Rarity.Uncommon => Rarity.Common,
            _ => null
        };
    }
}