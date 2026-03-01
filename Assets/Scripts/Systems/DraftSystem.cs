using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DraftSystem
{
    private readonly IRarityRoller _rarityRoller;
    private readonly IUpgradeRepository _upgradeRepository;
    private readonly IArtifactRepository _artifactRepository;

    public DraftSystem(IUpgradeRepository upgradeRepository)
        : this(upgradeRepository, null, new RarityRoller())
    {
    }

    public DraftSystem(IUpgradeRepository upgradeRepository, IRarityRoller rarityRoller)
        : this(upgradeRepository, null, rarityRoller)
    {
    }

    public DraftSystem(IUpgradeRepository upgradeRepository, IArtifactRepository artifactRepository)
        : this(upgradeRepository, artifactRepository, new RarityRoller())
    {
    }

    /// <summary>Creates an artifact-only draft system (for boss rewards).</summary>
    public DraftSystem(IArtifactRepository artifactRepository)
        : this(null, artifactRepository, new RarityRoller())
    {
    }

    /// <summary>Creates an artifact-only draft system (for boss rewards) with a custom rarity roller.</summary>
    public DraftSystem(IArtifactRepository artifactRepository, IRarityRoller rarityRoller)
        : this(null, artifactRepository, rarityRoller)
    {
    }

    public DraftSystem(IUpgradeRepository upgradeRepository, IArtifactRepository artifactRepository,
        IRarityRoller rarityRoller)
    {
        _upgradeRepository = upgradeRepository;
        _artifactRepository = artifactRepository;
        _rarityRoller = rarityRoller ?? throw new ArgumentNullException(nameof(rarityRoller));

        Log.Info("DraftSystem initialized", new
        {
            hasUpgradeRepository = upgradeRepository != null,
            hasArtifactRepository = artifactRepository != null,
            rarityRollerType = rarityRoller.GetType().Name
        });
    }

    public List<DraftOption> GenerateDraft(int count)
    {
        Log.Info("GenerateDraft called", new { count });

        try
        {
            var pool = BuildPool();
            Log.Info("Draft pool retrieved", new { poolSize = pool.Count });

            var result = new List<DraftOption>();

            // Roll rarity first
            var rolledRarity = _rarityRoller.RollRarity();

            for (var i = 0; i < count; i++)
            {
                var available = pool
                    .Where(o => !result.Contains(o))
                    .ToList();

                if (available.Count == 0)
                {
                    if (Application.isPlaying)
                        Log.Warning("No more draft options available", new { round = i });
                    break;
                }

                var selected = SelectOptionByRarity(available, rolledRarity);

                if (selected == null)
                {
                    Log.Warning("No option found for any rarity", new { round = i });
                    break;
                }

                Log.Info("Draft selection", new
                {
                    round = i,
                    selected = selected.DisplayName,
                    rarity = selected.GetRarity(),
                    rolledRarity,
                    remainingPool = available.Count - 1
                });

                result.Add(selected);
            }

            Log.Info("Draft generated", new
            {
                draftSize = result.Count,
                selectedOptions = string.Join(",", result.ConvertAll(o => o.DisplayName))
            });

            return result;
        }
        catch (Exception ex)
        {
            Log.Exception(ex, "GenerateDraft failed", new { count });
            throw;
        }
    }

    private List<DraftOption> BuildPool()
    {
        var pool = new List<DraftOption>();

        if (_upgradeRepository != null)
        {
            var upgrades = _upgradeRepository.GetAll();
            for (var i = 0; i < upgrades.Count; i++)
                pool.Add(new DraftOption(upgrades[i]));
        }

        if (_artifactRepository != null)
        {
            var artifacts = _artifactRepository.GetAll();
            for (var i = 0; i < artifacts.Count; i++)
                pool.Add(new DraftOption(artifacts[i]));
        }

        return pool;
    }

    private DraftOption SelectOptionByRarity(List<DraftOption> available, Rarity targetRarity)
    {
        var pool = available.Where(o => o.GetRarity() == targetRarity).ToList();

        if (pool.Count > 0) return pool[Random.Range(0, pool.Count)];

        var fallbackRarity = GetNextLowerRarity(targetRarity);
        if (fallbackRarity.HasValue)
        {
            Log.Info("Rarity fallback", new { from = targetRarity, to = fallbackRarity.Value });
            return SelectOptionByRarity(available, fallbackRarity.Value);
        }

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