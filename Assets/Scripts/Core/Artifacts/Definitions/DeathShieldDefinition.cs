using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Death Shield (Hourglass)")]
    public class DeathShieldDefinition : ArtifactDefinition
    {
        [SerializeField] private float _revivePercent = 0.5f;

        public override IArtifact CreateArtifact() => new DeathShield(_revivePercent);
    }
}
