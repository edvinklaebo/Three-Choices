using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Berserker Mask")]
    public class BerserkerMaskDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new BerserkerMask();
    }
}
