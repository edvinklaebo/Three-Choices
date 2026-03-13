using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Phantom Strike (Crown of Echoes)")]
    public class PhantomStrikeDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new PhantomStrike();
    }
}
