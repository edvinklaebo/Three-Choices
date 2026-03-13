using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    /// <summary>
    /// Definition used for both Poison Darts and Poisoned Blade artifacts –
    /// both apply the <see cref="PoisonAmplifier"/> passive.
    /// </summary>
    [CreateAssetMenu(menuName = "Artifacts/Poison Amplifier")]
    public class PoisonAmplifierDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new PoisonAmplifier();
    }
}
