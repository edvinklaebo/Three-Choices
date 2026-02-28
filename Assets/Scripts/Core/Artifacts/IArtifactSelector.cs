using System.Collections.Generic;

public interface IArtifactSelector
{
    ArtifactDefinition[] Select(IReadOnlyList<ArtifactDefinition> pool, int bossId, int count);
}
