using Core.Artifacts.Passives;
using UnityEngine;

namespace Core.Artifacts.Definitions
{
    [CreateAssetMenu(menuName = "Artifacts/Thorn Armor")]
    public class ThornArmorDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => new ThornArmor();
    }
}
