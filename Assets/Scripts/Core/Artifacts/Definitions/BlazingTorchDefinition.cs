using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Blazing Torch")]
    public class BlazingTorchDefinition : ArtifactDefinition
    {
        [SerializeField] private int _burnDuration = 3;
        [SerializeField] private int _burnDamage = 4;

        public override IArtifact CreateArtifact() => new BlazingTorch(_burnDuration, _burnDamage);
    }
}
