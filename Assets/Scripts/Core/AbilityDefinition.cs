using Core;
using Interfaces;
using UnityEngine;
using Utils;

/// <summary>
/// Abstract base for ability upgrade definitions.
/// Subclass this to implement a specific ability.
/// <para>
/// Override <see cref="Apply"/> to handle both first pickup (add the ability) and
/// duplicate pickups (upgrade it, e.g. increase damage). Use the
/// <see cref="FindExistingAbility{T}"/> helper to locate an already-owned instance.
/// </para>
/// </summary>
public abstract class AbilityDefinition : UpgradeDefinition
{
    [SerializeField] protected Sprite ProjectileSprite;

    /// <summary>
    /// Finds the first ability of type <typeparamref name="T"/> on the unit, or <c>null</c>.
    /// Use this in <see cref="UpgradeDefinition.Apply"/> to detect duplicate pickups.
    /// </summary>
    protected static T FindExistingAbility<T>(Unit unit) where T : class, IAbility
    {
        foreach (var ability in unit.Abilities)
            if (ability is T found)
                return found;
        return null;
    }

#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName)
    {
        id = identifier;
        displayName = soName;
    }
#endif
}
