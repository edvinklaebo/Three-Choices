using System.Collections.Generic;

namespace Core.Artifacts
{
    public interface IArtifactSelector
    {
        ArtifactDefinition[] Select(IReadOnlyList<ArtifactDefinition> pool, int bossId, int count);
    }
}
