using System.Collections.Generic;

namespace Core.Artifacts
{
    public interface IArtifactRepository
    {
        IReadOnlyList<ArtifactDefinition> GetAll();
    }
}
