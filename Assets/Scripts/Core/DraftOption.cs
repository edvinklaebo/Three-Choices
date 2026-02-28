using System;
using UnityEngine;

/// <summary>
/// Unified draft item that can represent either an <see cref="UpgradeDefinition"/> or an
/// <see cref="ArtifactDefinition"/>. Use <see cref="IsArtifact"/> to distinguish between the two.
/// </summary>
public class DraftOption
{
    public UpgradeDefinition Upgrade { get; }
    public ArtifactDefinition Artifact { get; }

    public bool IsArtifact => Artifact != null;

    public string DisplayName => IsArtifact ? Artifact.DisplayName : Upgrade.DisplayName;
    public string Description => IsArtifact ? Artifact.Description : Upgrade.Description;
    public Sprite Icon => IsArtifact ? Artifact.Icon : Upgrade.Icon;

    public Rarity GetRarity() => IsArtifact ? Artifact.Rarity : Upgrade.GetRarity();

    public DraftOption(UpgradeDefinition upgrade)
    {
        Upgrade = upgrade ?? throw new ArgumentNullException(nameof(upgrade));
    }

    public DraftOption(ArtifactDefinition artifact)
    {
        Artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));
    }
}
