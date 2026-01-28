using System;
using System.Collections.Generic;

public class DraftSystem
{
    private readonly IUpgradeRepository _repository;

    public DraftSystem(IUpgradeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        
        Log.Info("DraftSystem initialized", new
        {
            repositoryType = repository.GetType().Name
        });
    }

    public List<UpgradeDefinition> GenerateDraft(int count)
    {
        Log.Info("GenerateDraft called", new { count });

        try
        {
            var pool = _repository.GetAll();
            Log.Info("Upgrade pool retrieved", new { poolSize = pool.Count });

            var result = new List<UpgradeDefinition>();

            for (var i = 0; i < count && pool.Count > 0; i++)
            {
                var index = UnityEngine.Random.Range(0, pool.Count);
                var selected = pool[index];

                Log.Info("Draft selection", new
                {
                    round = i,
                    selected = selected?.DisplayName ?? "null",
                    remainingPool = pool.Count
                });

                result.Add(selected);
                pool.RemoveAt(index);
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
}