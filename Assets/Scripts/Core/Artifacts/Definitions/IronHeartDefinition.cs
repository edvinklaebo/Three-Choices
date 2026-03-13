using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Iron Heart")]
    public class IronHeartDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new IronHeart();
    }
}
