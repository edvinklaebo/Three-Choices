using System;
using UnityEngine;

/// <summary>
/// Unified draft item wrapping any <see cref="IDraftable"/> definition (Upgrade, Artifact, etc.).
/// Adding a new draftable type requires only that it implements <see cref="IDraftable"/> —
/// no changes to this class are needed.
/// </summary>
public class DraftOption
{
    public IDraftable Source { get; }

    public string DisplayName => Source.DisplayName;
    public string Description => Source.Description;
    public Sprite Icon => Source.Icon;
    public Rarity GetRarity() => Source.GetRarity();

    public DraftOption(IDraftable source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }
}
