using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Twin Blades")]
    public class TwinBladesDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new TwinBlades();
    }
}
