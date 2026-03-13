using Core.Artifacts;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Minimal concrete <see cref="ArtifactDefinition"/> used only in tests that need to
    /// exercise base-class property accessors (Id, DisplayName, Rarity, etc.) without
    /// actually applying an artifact to a unit.
    /// </summary>
    public class FakeArtifactDefinition : ArtifactDefinition
    {
        public override IArtifact CreateArtifact() => throw new System.NotImplementedException(
            "FakeArtifactDefinition is only for testing metadata – do not call CreateArtifact()");
    }
}
