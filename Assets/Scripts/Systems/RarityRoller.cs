using System.Collections.Generic;
using System.Linq;

public interface IRarityRoller
{
    Rarity RollRarity();
}

public class RarityRoller : IRarityRoller
{
    private readonly Dictionary<Rarity, int> _rarityWeights;
    private readonly int _totalWeight;

    public RarityRoller()
    {
        _rarityWeights = new Dictionary<Rarity, int>
        {
            { Rarity.Common, (int)Rarity.Common },
            { Rarity.Uncommon, (int)Rarity.Uncommon },
            { Rarity.Rare, (int)Rarity.Rare },
            { Rarity.Epic, (int)Rarity.Epic }
        };
        
        _totalWeight = _rarityWeights.Values.Sum();
        
        Log.Info("RarityRoller initialized", new
        {
            weights = string.Join(", ", _rarityWeights.Select(kvp => $"{kvp.Key}={kvp.Value}")),
            totalWeight = _totalWeight
        });
    }

    public Rarity RollRarity()
    {
        var roll = UnityEngine.Random.Range(0, _totalWeight);
        
        var cumulative = 0;
        foreach (var kvp in _rarityWeights.OrderByDescending(x => x.Value))
        {
            cumulative += kvp.Value;
            if (roll < cumulative)
            {
                Log.Info("Rarity rolled", new { rarity = kvp.Key, roll, cumulative, totalWeight = _totalWeight });
                return kvp.Key;
            }
        }
        
        // Fallback (shouldn't reach here)
        Log.Warning("Rarity roll fallback to Common", new { roll, totalWeight = _totalWeight });
        return Rarity.Common;
    }
}
