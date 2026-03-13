using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Heart of Oak")]
    public class HeartOfOakDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new HeartOfOak();
    }
}
