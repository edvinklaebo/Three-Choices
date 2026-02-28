using System.Collections.Generic;

public interface IArtifactRepository
{
    IReadOnlyList<ArtifactDefinition> GetAll();
}
