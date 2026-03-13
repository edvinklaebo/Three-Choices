using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Quickboots")]
    public class QuickbootsDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new Quickboots();
    }
}
