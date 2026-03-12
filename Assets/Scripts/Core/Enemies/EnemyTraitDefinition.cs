using Core;
using UnityEngine;

/// <summary>
///     Abstract base class for enemy traits. Traits are stateless ScriptableObjects that
///     attach behaviors to a runtime <see cref="Unit"/> at creation time.
///     Create concrete traits via ScriptableObject asset menus.
/// </summary>
public abstract class EnemyTraitDefinition : ScriptableObject
{
    /// <summary>
    ///     Attaches this trait's behavior to the given unit. Called exactly once per unit
    ///     at creation time. Units are not reused, so the event subscription lifetime
    ///     matches the unit's lifetime.
    /// </summary>
    public abstract void Apply(Unit unit);
}
