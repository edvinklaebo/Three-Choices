using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Corrupted Tome")]
    public class CorruptedTomeDefinition : ArtifactDefinition
    {
        [SerializeField] private float _multiplier = 1.3f;

        public override IArtifact CreateArtifact() => new CorruptedTome(_multiplier);
    }
}
