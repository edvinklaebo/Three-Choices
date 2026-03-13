using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Vampiric Fang")]
    public class VampiricFangDefinition : ArtifactDefinition
    {
        [SerializeField] private float _percent = 0.2f;

        public override IArtifact CreateArtifact() => new VampiricFang(_percent);
    }
}
