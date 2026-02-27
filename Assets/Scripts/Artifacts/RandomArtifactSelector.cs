using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RandomArtifactSelector : IArtifactSelector
{
    public ArtifactDefinition[] Select(IReadOnlyList<ArtifactDefinition> pool, int bossId, int count)
    {
        var available = new List<ArtifactDefinition>(pool);
        var result = new ArtifactDefinition[System.Math.Min(count, available.Count)];

        for (var i = 0; i < result.Length; i++)
        {
            var index = Random.Range(0, available.Count);
            result[i] = available[index];
            // Swap-with-last O(n) removal instead of O(n²) shift
            available[index] = available[available.Count - 1];
            available.RemoveAt(available.Count - 1);
        }

        return result;
    }
}
