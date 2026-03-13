using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/War Gauntlet")]
    public class WarGauntletDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new WarGauntlet();
    }
}
