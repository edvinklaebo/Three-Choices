using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Artifacts/Artifact Pool")]
public class ArtifactPool : ScriptableObject, IArtifactRepository
{
    private List<ArtifactDefinition> _artifacts;

    private void OnEnable()
    {
        var loaded = Resources.LoadAll<ArtifactDefinition>("Artifacts");
        _artifacts = new List<ArtifactDefinition>(loaded);

        Log.Info($"[ArtifactPool] Loaded {_artifacts.Count} artifact definitions");
    }

    public IReadOnlyList<ArtifactDefinition> GetAll()
    {
        if (_artifacts == null)
        {
            var loaded = Resources.LoadAll<ArtifactDefinition>("Artifacts");
            _artifacts = new List<ArtifactDefinition>(loaded);
            Log.Info($"[ArtifactPool] Lazy-loaded {_artifacts.Count} artifact definitions");
        }

        return _artifacts;
    }
}
