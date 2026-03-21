using System.Collections.Generic;
using Core;
using Interfaces;
using UnityEngine;

/// <summary>
///     Abstract base for all ability upgrade ScriptableObjects.
///     Holds both the balance data (damage, cooldown, tags) and the upgrade-card
///     identity fields inherited from <see cref="UpgradeDefinition"/>.
///     Concrete subclasses (e.g. <see cref="Core.Abilities.Definitions.FireballDefinition"/>,
///     <see cref="Core.Abilities.Definitions.ArcaneMissilesDefinition"/>) live in
///     Core/Abilities/Definitions/ and override <see cref="UpgradeDefinition.Apply"/> to handle
///     first-pickup creation and duplicate stacking.
/// </summary>
public abstract class AbilityDefinition : UpgradeDefinition
{
    [Header("Damage")]
    [Tooltip("Base damage dealt per cast (or per missile for multi-hit abilities).")]
    [Min(1)] [SerializeField] private int _baseDamage = 3;
    [Tooltip("Damage added to the runtime instance each time the player upgrades this ability.")]
    [Min(1)] [SerializeField] private int _damagePerUpgrade = 1;

    [Header("Timing")]
    [Tooltip("0 = fires every turn. N = must wait N turns between casts.")]
    [Min(0)] [SerializeField] private int _cooldownRounds;

    [Header("Tags")]
    [SerializeField] private List<string> _tags = new();

    public int BaseDamage => _baseDamage;
    public int DamagePerUpgrade => _damagePerUpgrade;
    public int CooldownRounds => _cooldownRounds;
    public IReadOnlyList<string> Tags => _tags;

    /// <summary>Creates a ready-to-use runtime ability seeded from this definition.</summary>
    public abstract IAbility CreateRuntimeAbility();

    /// <summary>
    ///     Returns the first ability of type <typeparamref name="T"/> on the unit, or null.
    ///     Use this in <see cref="UpgradeDefinition.Apply"/> to detect whether the ability is
    ///     already owned before creating a new instance.
    /// </summary>
    protected static T FindExistingAbility<T>(Unit unit) where T : class, IAbility
    {
        foreach (var ability in unit.Abilities)
            if (ability is T found)
                return found;
        return null;
    }

#if UNITY_EDITOR
    public void EditorInitAbility(int baseDamage, int damagePerUpgrade, int cooldownRounds)
    {
        _baseDamage = baseDamage;
        _damagePerUpgrade = damagePerUpgrade;
        _cooldownRounds = cooldownRounds;
    }

    public void EditorInitTags(string[] tags)
    {
        _tags = new List<string>(tags);
    }
#endif
}