using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Blood Ritual")]
    public class BloodRitualDefinition : ArtifactDefinition
    {
        [SerializeField] private int _bleedStacks = 2;
        [SerializeField] private int _bleedDuration = 3;
        [SerializeField] private int _bleedDamage = 2;

        public override IArtifact CreateArtifact() => new BloodRitual(_bleedStacks, _bleedDuration, _bleedDamage);
    }
}
