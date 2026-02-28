using UnityEngine;

/// <summary>
/// Marks a ScriptableObject definition as eligible to appear in a draft.
/// Implement this on any definition type (Upgrade, Artifact, Curse, etc.) to
/// make it automatically available to <see cref="DraftSystem"/> and
/// <see cref="DraftOption"/> without changing either class.
/// </summary>
public interface IDraftable
{
    string DisplayName { get; }
    string Description { get; }
    Sprite Icon { get; }
    Rarity GetRarity();
}
