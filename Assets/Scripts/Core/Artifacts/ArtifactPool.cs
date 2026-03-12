using System.Collections.Generic;

using UnityEngine;

using Utils;

namespace Core.Artifacts
{
    [CreateAssetMenu(menuName = "Artifacts/Artifact Pool")]
    public class ArtifactPool : ScriptableObject, IArtifactRepository
    {
        private List<ArtifactDefinition> _artifacts;

        private void OnEnable()
        {
            var loaded = Resources.LoadAll<ArtifactDefinition>("Artifacts");
            this._artifacts = new List<ArtifactDefinition>(loaded);

            Log.Info($"[ArtifactPool] Loaded {this._artifacts.Count} artifact definitions");
        }

        public IReadOnlyList<ArtifactDefinition> GetAll()
        {
            if (this._artifacts == null)
            {
                var loaded = Resources.LoadAll<ArtifactDefinition>("Artifacts");
                this._artifacts = new List<ArtifactDefinition>(loaded);
                Log.Info($"[ArtifactPool] Lazy-loaded {this._artifacts.Count} artifact definitions");
            }

            return this._artifacts;
        }
    }
}
