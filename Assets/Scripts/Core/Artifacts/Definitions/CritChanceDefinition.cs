using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Crit Chance (Lucky Horseshoe)")]
    public class CritChanceDefinition : ArtifactDefinition
    {
        [SerializeField] private float _critChance = 0.1f;

        public override IArtifact CreateArtifact() => new CritChance(_critChance);
    }
}
